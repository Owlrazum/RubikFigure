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

    private int2 _sidesRingsCount;
    private int _segmentResolution;
    private int4 _transSegsCount;

    public override void StartGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency)
    {
        _sidesRingsCount = quadStripsCollection.Dims;
        _segmentResolution = quadStripsCollection.GetQuadIndexer(0).y;

        int PerSideTransitionsCount = _sidesRingsCount.x * 2;
        int PerRingTranstionsCount = _sidesRingsCount.y * 2;
        int totalTransitionsCount = PerSideTransitionsCount + PerRingTranstionsCount;

        PerSideTransSegCount = VerticalTransSegCount * (_sidesRingsCount.y - 1) + LevitationTransSegCount;
        PerRingTransSegCount = ClockOrderTransSegCount * _sidesRingsCount.x;


        _transSegsCount.x = VerticalTransSegCount * 2 * (_sidesRingsCount.y - 1) * _sidesRingsCount.x;
        _transSegsCount.y = LevitationTransSegCount * 2 * _sidesRingsCount.x;
        _transSegsCount.z = ClockOrderTransSegCount * 2 * _sidesRingsCount.x * _sidesRingsCount.y;
        _transSegsCount.w = _transSegsCount.x + _transSegsCount.y + _transSegsCount.z;


        int OT_sideTransitions = PerSideTransitionsCount * _sidesRingsCount.y;
        int OT_ringTransitions = PerRingTranstionsCount * _sidesRingsCount.x;

        NativeArray<int2> OT_buffer =new NativeArray<int2>(OT_sideTransitions + OT_ringTransitions, Allocator.TempJob);
        NativeArray<int2> OT_bufferIndexers = new NativeArray<int2>(totalTransitionsCount, Allocator.TempJob);
        SegmentedBufferInt2 OT = new SegmentedBufferInt2(OT_buffer, OT_bufferIndexers);

        NativeArray<int2> QS_TransitionsBufferIndexers = new NativeArray<int2>(totalTransitionsCount, Allocator.Persistent);
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
        _dataJobHandle = transitionDataJob.ScheduleParallel(totalTransitionsCount, 32, dependency);
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

    private int GetIndex(int side, int ring)
    {
        return XyToIndex(side, ring, _sidesRingsCount.y);
    }

    private void IncreaseRing(ref int ring)
    {
        if (ring + 1 >= _sidesRingsCount.y)
        {
            ring = 0;
        }
        else
        {
            ring++;
        }
    }

    private void IncreaseSide(ref int side)
    {
        if (side + 1 >= _sidesRingsCount.x)
        {
            side = 0;
        }
        else
        {
            side++;
        }
    }

    public override void FinishGeneration(Figure figure)
    {
        _dataJobHandle.Complete();

        Wheel wheel = figure as Wheel;
        Assert.IsNotNull(wheel);

        Array2D<WheelSegmentTransitions> transitionDatas =
            new Array2D<WheelSegmentTransitions>(new int2(_sidesRingsCount.x, _sidesRingsCount.y));
        for (int i = 0; i < _transitionsCollection.QSTransSegsCount; i += 2)
        {
            QS_Transition clockWiseTransition = _transitionsCollection.GetQSTransition(i);
            QS_Transition antiClockWiseTransition = _transitionsCollection.GetQSTransition(i + 1);

            // ValknutSegmentTransitions transData = new ValknutSegmentTransitions();
            // ValknutSegmentTransitions.Clockwise(ref transData) = clockWiseTransition;
            // ValknutSegmentTransitions.AntiClockwise(ref transData) = antiClockWiseTransition;

            // int2 originTargetIndex = _originTargetIndices[i];
            // int2 segmentIndex = new int2(originTargetIndex.y / Valknut.PartsCount, originTargetIndex.y % Valknut.PartsCount);
            // transitionDatas[segmentIndex] = transData;
        }

        wheel.AssignTransitionDatas(transitionDatas);
        // Array2D<WheelSegmentTransitions> transitionDatas = new Array2D<WheelSegmentTransitions>(_sidesRingsCount);
        // int2x3 index = int2x3.zero;
        // for (int side = 0; side < _sidesRingsCount.x; side++)
        // {
        //     for (int ring = 0; ring < _sidesRingsCount.y; ring++)
        //     {
        //         NativeArray<QSTransSegment> atsi = _atsi.GetSubArray(index[0].x, ClockOrderTransSegCount);
        //         NativeArray<QSTransSegment> ctsi = _ctsi.GetSubArray(index[0].x, ClockOrderTransSegCount);
        //         index[0].x += ClockOrderTransSegCount;

        //         NativeArray<QSTransSegment> dtsi;
        //         if (ring == 0)
        //         {
        //             dtsi = _levDtsi.GetSubArray(index[1].x, LevitationTransSegCount);
        //             index[1].x += LevitationTransSegCount;
        //         }
        //         else
        //         {
        //             dtsi = _dtsi.GetSubArray(index[2].x, VerticalTransSegCountPerQuad * _segmentResolution);
        //             index[2].x += VerticalTransSegCountPerQuad * _segmentResolution;
        //         }

        //         NativeArray<QSTransSegment> utsi;
        //         if (ring == _sidesRingsCount.y - 1)
        //         {
        //             utsi = _levUtsi.GetSubArray(index[1].y, LevitationTransSegCount);
        //             index[1].y += LevitationTransSegCount;
        //         }
        //         else
        //         {
        //             utsi = _utsi.GetSubArray(index[2].y, VerticalTransSegCountPerQuad * _segmentResolution);
        //             index[2].y += VerticalTransSegCountPerQuad * _segmentResolution;
        //         }

        //         WheelSegmentTransitions transData = new WheelSegmentTransitions();
        //         WheelSegmentTransitions.Atsi(ref transData) = new QSTransition(atsi);
        //         WheelSegmentTransitions.Ctsi(ref transData) = new QSTransition(ctsi);
        //         WheelSegmentTransitions.Dtsi(ref transData) = new QSTransition(dtsi);
        //         WheelSegmentTransitions.Utsi(ref transData) = new QSTransition(utsi);

        //         transitionDatas[side, ring] = transData;
        //     }
        // }
        // _wheel.AssignTransitionDatas(transitionDatas);
        // throw new System.NotImplementedException();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        // _originsTargetsIndices.DisposeIfNeeded();
    }
}
/*
_transitionGrid = new NativeArray<float3>(
        _sidesRingsCount.x * (_sidesRingsCount.y + 1) * _segmentResolution, Allocator.TempJob);

    _dtsi = new NativeArray<QSTransSegment>(_transSegsCount.x, Allocator.Persistent);
    _utsi = new NativeArray<QSTransSegment>(_transSegsCount.x, Allocator.Persistent);

    _levDtsi = new NativeArray<QSTransSegment>(_transSegsCount.y, Allocator.Persistent);
    _levUtsi = new NativeArray<QSTransSegment>(_transSegsCount.y, Allocator.Persistent);

    _ctsi = new NativeArray<QSTransSegment>(_transSegsCount.z, Allocator.Persistent);
    _atsi = new NativeArray<QSTransSegment>(_transSegsCount.z, Allocator.Persistent);
*/