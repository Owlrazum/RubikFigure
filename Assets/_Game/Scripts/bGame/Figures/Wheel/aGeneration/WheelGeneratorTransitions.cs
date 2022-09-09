using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Collections;
using static Orazum.Collections.IndexUtilities;

using Orazum.Math;

using static Orazum.Meshing.BufferUtils;

// OT - origin target
// QSTS - quad strip transition segment
public class WheelGeneratorTransitions : FigureGeneratorTransitions
{
    private const int LevitationTransSegCount = 3;
    private const int ClockOrderTransSegCount = 1;
    private const int VerticalTransSegCount= 1;

    private int PerSideTransSegCount;
    private int PerRingTransSegCount;

    private int PerSideTransitionsCount;
    private int TotalTransitionsCount;

    private int2 _sidesRingsCount;
    private int _segmentResolution;
    private int4 _transSegsCount;

    protected override void StartTransitionsGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency)
    {
        _sidesRingsCount = quadStripsCollection.Dims;
        _segmentResolution = quadStripsCollection.GetIndexer(0).y - 1;

        PerSideTransitionsCount = _sidesRingsCount.x * 2;
        int PerRingTransitionsCount = _sidesRingsCount.y * 2;
        TotalTransitionsCount = PerSideTransitionsCount + PerRingTransitionsCount;

        PerSideTransSegCount = VerticalTransSegCount * (_sidesRingsCount.y - 1) + LevitationTransSegCount;
        PerRingTransSegCount = ClockOrderTransSegCount * _sidesRingsCount.x;


        _transSegsCount.x = VerticalTransSegCount * 2 * (_sidesRingsCount.y - 1) * _sidesRingsCount.x;
        _transSegsCount.y = LevitationTransSegCount * 2 * _sidesRingsCount.x;
        _transSegsCount.z = ClockOrderTransSegCount * 2 * _sidesRingsCount.x * _sidesRingsCount.y;
        _transSegsCount.w = _transSegsCount.x + _transSegsCount.y + _transSegsCount.z;


        int OT_sideTransitions = PerSideTransitionsCount * _sidesRingsCount.y;
        int OT_ringTransitions = PerRingTransitionsCount * _sidesRingsCount.x;

        NativeArray<int2> OT_buffer =new NativeArray<int2>(OT_sideTransitions + OT_ringTransitions, Allocator.TempJob);
        NativeArray<int2> OT_bufferIndexers = new NativeArray<int2>(TotalTransitionsCount, Allocator.TempJob);
        SegmentedBufferInt2 OT = new SegmentedBufferInt2(OT_buffer, OT_bufferIndexers);

        NativeArray<int2> QS_TransitionsBufferIndexers = new NativeArray<int2>(TotalTransitionsCount, Allocator.Persistent);
        GenerateDataJobIndexData(ref OT, ref QS_TransitionsBufferIndexers);

        NativeArray<QST_Segment> QS_TransitionsBuffer = new NativeArray<QST_Segment>(_transSegsCount.w, Allocator.Persistent);
        _transitionsCollection = new QS_TransitionsBuffer(QS_TransitionsBuffer, QS_TransitionsBufferIndexers);

        WheelGenJobTransData transitionDataJob = new WheelGenJobTransData()
        {
            P_VertOrderTransitionsCount = PerSideTransitionsCount,
            InQuadStripsCollection = quadStripsCollection,
            InOriginsTargetsIndices = OT,
            OutTransitionsCollection = _transitionsCollection
        };
        _dataJobHandle = transitionDataJob.ScheduleParallel(TotalTransitionsCount, 32, dependency);
        OT.Dispose(_dataJobHandle);
    }

    private void GenerateDataJobIndexData(
        ref SegmentedBufferInt2 OT,
        ref NativeArray<int2> QS_TransitionsBuffersIndexers)
    {
        int QSTS_indexer = 0;
        int2 QS_TransitionsBufferIndexer = new int2(0, PerSideTransSegCount);

        int OT_indexer = 0;
        int2 OT_bufferIndexer = new int2(0, _sidesRingsCount.y);
        for (int side = 0; side < _sidesRingsCount.x; side++)
        {
            QS_TransitionsBuffersIndexers[QSTS_indexer++] = QS_TransitionsBufferIndexer;
            MoveBufferIndexer(ref QS_TransitionsBufferIndexer, PerSideTransSegCount);
            var upBuffer = OT.GetBufferSegmentAndWriteIndexer(OT_bufferIndexer, OT_indexer++);
            MoveBufferIndexer(ref OT_bufferIndexer, _sidesRingsCount.y);

            QS_TransitionsBuffersIndexers[QSTS_indexer++] = QS_TransitionsBufferIndexer;
            MoveBufferIndexer(ref QS_TransitionsBufferIndexer, PerSideTransSegCount);
            var downBuffer = OT.GetBufferSegmentAndWriteIndexer(OT_bufferIndexer, OT_indexer++);
            MoveBufferIndexer(ref OT_bufferIndexer, _sidesRingsCount.y);

            int prevRing = _sidesRingsCount.y - 1;
            int ring = 0;
            int nextRing = 1;

            int bottomIndex = GetIndex(side, prevRing);
            int middleIndex = GetIndex(side, ring);
            int upperIndex = GetIndex(side, nextRing);

            while (ring < _sidesRingsCount.y)
            {
                upBuffer[ring] = new int2(bottomIndex, middleIndex);
                downBuffer[ring] = new int2(upperIndex, middleIndex);
                IncreaseRing(ref prevRing);
                IncreaseRing(ref ring);
                IncreaseRing(ref nextRing);
            }
        }

        for (int ring = 0; ring < _sidesRingsCount.y; ring++)
        {
            QS_TransitionsBuffersIndexers[QSTS_indexer++] = QS_TransitionsBufferIndexer;
            MoveBufferIndexer(ref QS_TransitionsBufferIndexer, PerRingTransSegCount);
            var cwBuffer = OT.GetBufferSegmentAndWriteIndexer(OT_bufferIndexer, OT_indexer++);
            MoveBufferIndexer(ref OT_bufferIndexer, _sidesRingsCount.x);

            QS_TransitionsBuffersIndexers[QSTS_indexer++] = QS_TransitionsBufferIndexer;
            MoveBufferIndexer(ref QS_TransitionsBufferIndexer, PerRingTransSegCount);
            var antiCwBuffer = OT.GetBufferSegmentAndWriteIndexer(OT_bufferIndexer, OT_indexer++);
            MoveBufferIndexer(ref OT_bufferIndexer, _sidesRingsCount.x);

            int prevSide = _sidesRingsCount.x - 1;
            int side = 0;
            int nextSide = 1;

            int leftIndex = GetIndex(prevSide, ring);
            int middleIndex = GetIndex(side, ring);
            int rightIndex = GetIndex(nextSide, ring);

            while (side < _sidesRingsCount.x)
            {
                cwBuffer[side] = new int2(leftIndex, middleIndex);
                antiCwBuffer[side] = new int2(rightIndex, middleIndex);
                IncreaseSide(ref prevSide);
                IncreaseSide(ref side);
                IncreaseSide(ref nextSide);
            }
        }
    }

    protected override void FinishTransitionsGeneration(Figure figure)
    {
        _dataJobHandle.Complete();

        Wheel wheel = figure as Wheel;
        Assert.IsNotNull(wheel);

        Array2D<WheelSegmentTransitions> transitionDatas =
            new Array2D<WheelSegmentTransitions>(new int2(_sidesRingsCount.x, _sidesRingsCount.y));

        int sideIndexer = 0;
        for (int i = 0; i < PerSideTransitionsCount; i += 2)
        {
            QS_Transition upTransition = _transitionsCollection.GetQSTransition(i);
            QS_Transition downTransition = _transitionsCollection.GetQSTransition(i + 1);

            int2 upIndexer = new int2(0, 3);
            int2 downIndexer = new int2(0, 1);
            for (int ring = 0; ring < _sidesRingsCount.y; ring++)
            { 
                WheelSegmentTransitions transData = new WheelSegmentTransitions();
                upTransition.GetSubTransition(upIndexer, out transData.Up);
                downTransition.GetSubTransition(downIndexer, out transData.Down);
                
                MoveBufferIndexer(ref upIndexer, 1);
                MoveBufferIndexer(ref downIndexer, i == ring - 1 ? 3 : 1);
                transitionDatas[sideIndexer, ring] = transData;
            }

            sideIndexer++;
        }

        int ringIndexer = 0;
        for (int i = PerSideTransitionsCount; i < TotalTransitionsCount; i += 2)
        {
            QS_Transition cwTransition = _transitionsCollection.GetQSTransition(i);
            QS_Transition antiCWTransition = _transitionsCollection.GetQSTransition(i + 1);

            int2 cwIndexer = new int2(0, 2);
            int2 antiCWIndexer = new int2(0, 2);
            for (int side = 0; side < _sidesRingsCount.x; side++)
            {
                WheelSegmentTransitions transData = transitionDatas[side, ringIndexer];
                cwTransition.GetSubTransition(cwIndexer, out transData.CW);
                antiCWTransition.GetSubTransition(antiCWIndexer, out transData.AntiCW);

                MoveBufferIndexer(ref cwIndexer, 2);
                MoveBufferIndexer(ref antiCWIndexer, 2);
            }

            ringIndexer++;
        }

        wheel.AssignTransitionDatas(transitionDatas);
    }

    private int GetIndex(int side, int ring)
    {
        return XyToIndex(side, ring, _sidesRingsCount.y);
    }

    private void IncreaseRing(ref int ring)
    {
        IncreaseIndex(ref ring, _sidesRingsCount.y);
    }

    private void IncreaseSide(ref int side)
    {
        IncreaseIndex(ref side, _sidesRingsCount.x);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}