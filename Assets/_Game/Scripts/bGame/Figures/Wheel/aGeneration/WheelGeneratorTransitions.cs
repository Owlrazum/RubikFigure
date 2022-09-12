using System;

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
    private const int ClockOrderTransSegCount = 2;
    private const int VerticalTransSegCount= 1;

    private int2 _sidesRingsCount;
    private int3 _transitionsCounts;
    private int3 _transSegsCounts;

    private int _resolution;

    private struct OT_NativeData : IDisposable
    { 
        public NativeArray<int2> SegmentsBuffer;
        public NativeArray<int2> IndexersBuffer;
        public SegmentedBufferInt2 OriginTargetsBuffer;

        public void Dispose()
        { 
            OriginTargetsBuffer.Dispose();
        }

        public void Dispose(JobHandle jobHandle)
        {
            OriginTargetsBuffer.Dispose(jobHandle);
        }
    }

    protected override void StartTransitionsGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency)
    {
        InitializeCounts(quadStripsCollection.Dims);
        PrepareNativeData(out OT_NativeData ot_data);

        _resolution = quadStripsCollection.GetQuadStrip(0).QuadsCount;

        WheelGenJobTransData transitionDataJob = new WheelGenJobTransData()
        {
            P_VertOrderTransitionsCount = _transitionsCounts.x,
            P_SegmentResolution = _resolution,
            P_SideCount = _sidesRingsCount.x,
            P_RingCount = _sidesRingsCount.y,

            InQuadStripsCollection = quadStripsCollection,
            InOriginsTargetsIndicesBuffer = ot_data.OriginTargetsBuffer,
            OutTransitionsBuffer = qst_data.TransitionsBuffer
        };
        _jobHandle = transitionDataJob.ScheduleParallel(_transitionsCounts.z, 32, dependency);
        ot_data.Dispose(_jobHandle);
    }

    private void InitializeCounts(int2 sidesRingsCount)
    { 
        _sidesRingsCount = sidesRingsCount;

        _transitionsCounts.x = _sidesRingsCount.x * 2;
        _transitionsCounts.y = _sidesRingsCount.y * 2;
        _transitionsCounts.z = _transitionsCounts.x + _transitionsCounts.y;

        _transSegsCounts.x = ClockOrderTransSegCount * _sidesRingsCount.x;

        _transSegsCounts.y = VerticalTransSegCount * (_sidesRingsCount.y - 1) + LevitationTransSegCount;
        _transSegsCounts.z = _transSegsCounts.x * 2 * _sidesRingsCount.y + _transSegsCounts.y * 2 * _sidesRingsCount.x;
    }

    private void PrepareNativeData(out OT_NativeData ot_data)
    {
        ot_data = new OT_NativeData();
        
        int OT_bufferLength = _transitionsCounts.x * _sidesRingsCount.y + _transitionsCounts.y * _sidesRingsCount.x;
        ot_data.SegmentsBuffer = new NativeArray<int2>(OT_bufferLength, Allocator.TempJob);
        ot_data.IndexersBuffer = new NativeArray<int2>(_transitionsCounts.z, Allocator.TempJob);
        ot_data.OriginTargetsBuffer = new SegmentedBufferInt2(ot_data.SegmentsBuffer, ot_data.IndexersBuffer);

        qst_data.SegmentsBuffer = new NativeArray<QST_Segment>(_transSegsCounts.z, Allocator.Persistent);
        qst_data.IndexersBuffer = new NativeArray<int2>(_transitionsCounts.z, Allocator.Persistent);
        qst_data.TransitionsBuffer = new QS_TransitionsBuffer(qst_data.SegmentsBuffer, qst_data.IndexersBuffer);

        int qsts_indexer = 0;
        int2 qst_bufferIndexer = int2.zero;

        int ot_indexer = 0;
        int2 ot_bufferIndexer = int2.zero;
        for (int side = 0; side < _sidesRingsCount.x; side++)
        {
            MoveBufferIndexer(ref qst_bufferIndexer, _transSegsCounts.y);
            qst_data.IndexersBuffer[qsts_indexer++] = qst_bufferIndexer;
            MoveBufferIndexer(ref ot_bufferIndexer, _sidesRingsCount.y);
            var upBuffer = ot_data.OriginTargetsBuffer.GetBufferSegmentAndWriteIndexer(ot_bufferIndexer, ot_indexer++);

            MoveBufferIndexer(ref qst_bufferIndexer, _transSegsCounts.y);
            qst_data.IndexersBuffer[qsts_indexer++] = qst_bufferIndexer;
            MoveBufferIndexer(ref ot_bufferIndexer, _sidesRingsCount.y);
            var downBuffer = ot_data.OriginTargetsBuffer.GetBufferSegmentAndWriteIndexer(ot_bufferIndexer, ot_indexer++);

            int prevRing = _sidesRingsCount.y - 1;
            int ring = 0;
            int nextRing = 1;

            int bottomIndex = GetIndex(side, prevRing);
            int middleIndex = GetIndex(side, ring);
            int upperIndex = GetIndex(side, nextRing);

            for (int ringIndexing = 0; ringIndexing < _sidesRingsCount.y; ringIndexing++)
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
            MoveBufferIndexer(ref qst_bufferIndexer, _transSegsCounts.x);
            qst_data.IndexersBuffer[qsts_indexer++] = qst_bufferIndexer;
            MoveBufferIndexer(ref ot_bufferIndexer, _sidesRingsCount.x);
            var cwBuffer = ot_data.OriginTargetsBuffer.GetBufferSegmentAndWriteIndexer(ot_bufferIndexer, ot_indexer++);

            MoveBufferIndexer(ref qst_bufferIndexer, _transSegsCounts.x);
            qst_data.IndexersBuffer[qsts_indexer++] = qst_bufferIndexer;
            MoveBufferIndexer(ref ot_bufferIndexer, _sidesRingsCount.x);
            var antiCwBuffer = ot_data.OriginTargetsBuffer.GetBufferSegmentAndWriteIndexer(ot_bufferIndexer, ot_indexer++);

            int prevSide = _sidesRingsCount.x - 1;
            int side = 0;
            int nextSide = 1;

            int leftIndex = GetIndex(prevSide, ring);
            int middleIndex = GetIndex(side, ring);
            int rightIndex = GetIndex(nextSide, ring);

            for (int sideIndexing = 0; sideIndexing < _sidesRingsCount.x; sideIndexing++)
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
        _jobHandle.Complete();

        Wheel wheel = figure as Wheel;
        Assert.IsNotNull(wheel);

        Array2D<WheelSegmentTransitions> transitionDatas = new(_sidesRingsCount);

        int sideIndexer = 0;
        for (int i = 0; i < _transitionsCounts.x; i += 2)
        {
            QS_Transition upTransition = qst_data.TransitionsBuffer.GetQSTransition(i);
            QS_Transition downTransition = qst_data.TransitionsBuffer.GetQSTransition(i + 1);

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
        for (int i = _transitionsCounts.x; i < _transitionsCounts.z; i += 2)
        {
            QS_Transition cwTransition = qst_data.TransitionsBuffer.GetQSTransition(i);
            QS_Transition antiCWTransition = qst_data.TransitionsBuffer.GetQSTransition(i + 1);

            int2 cwIndexer = new int2(0, 2);
            int2 antiCWIndexer = new int2(0, 2);
            for (int side = 0; side < _sidesRingsCount.x; side++)
            {
                WheelSegmentTransitions transData = transitionDatas[side, ringIndexer];
                cwTransition.GetSubTransition(cwIndexer, out transData.CW);
                antiCWTransition.GetSubTransition(antiCWIndexer, out transData.AntiCW);

                MoveBufferIndexer(ref cwIndexer, 2);
                MoveBufferIndexer(ref antiCWIndexer, 2);

                transitionDatas[side, ringIndexer] = transData;
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