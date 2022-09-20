using System;
using System.Collections;

using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;

using static Orazum.Meshing.BufferUtils;

// OT - origin target
// QSTS - quad strip transition segment
public class WheelGeneratorTransitions : FigureGeneratorTransitions
{
    private const int ClockOrderTransSegCount = 2;
    private const int VerticalTransSegCount = 2;

    private int2 _sidesRingsCount;
    private int3 _transSegsCounts;
    private int doubleTransitionsCount;

    private int _resolution;

    protected override void StartTransitionsGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency)
    {
        // StartCoroutine(DebugQuadStrips(quadStripsCollection));
        InitializeCounts(quadStripsCollection.Dims);
        PrepareNativeData();

        _resolution = quadStripsCollection.GetQuadStrip(0).QuadsCount;

        WheelGenJobTransData transitionDataJob = new WheelGenJobTransData()
        {
            P_VertOrderDoubleTransitionsCount = _sidesRingsCount.x,
            P_SegmentResolution = _resolution,
            P_SidesRingsCount = _sidesRingsCount,

            InQuadStripsCollection = quadStripsCollection,
            OutTransitionsBuffer = qst_data.TransitionsBuffer
        };
        _jobHandle = transitionDataJob.ScheduleParallel(doubleTransitionsCount, 32, dependency);
    }

    private void InitializeCounts(int2 sidesRingsCount)
    {
        _sidesRingsCount = sidesRingsCount;

        doubleTransitionsCount = _sidesRingsCount.x + _sidesRingsCount.y;
    }

    private void PrepareNativeData()
    {
        _transSegsCounts.x = VerticalTransSegCount * _sidesRingsCount.y * 2;
        _transSegsCounts.y = ClockOrderTransSegCount * _sidesRingsCount.x * 2;
        _transSegsCounts.z = _transSegsCounts.x * _sidesRingsCount.x + _transSegsCounts.y * _sidesRingsCount.y;

        qst_data.SegmentsBuffer = new NativeArray<QST_Segment>(_transSegsCounts.z, Allocator.Persistent);
        qst_data.IndexersBuffer = new NativeArray<int2>(doubleTransitionsCount, Allocator.Persistent);
        qst_data.TransitionsBuffer = new QS_TransitionsBuffer(qst_data.SegmentsBuffer, qst_data.IndexersBuffer);

        int qsts_indexer = 0;
        int2 qst_bufferIndexer = int2.zero;

        for (int side = 0; side < _sidesRingsCount.x; side++)
        {
            MoveBufferIndexer(ref qst_bufferIndexer, _transSegsCounts.x);
            qst_data.IndexersBuffer[qsts_indexer++] = qst_bufferIndexer;
        }

        for (int ring = 0; ring < _sidesRingsCount.y; ring++)
        {
            MoveBufferIndexer(ref qst_bufferIndexer, _transSegsCounts.y);
            qst_data.IndexersBuffer[qsts_indexer++] = qst_bufferIndexer;
        }
    }

    protected override void FinishTransitionsGeneration(Figure figure)
    {
        _jobHandle.Complete();

        Wheel wheel = figure as Wheel;
        Assert.IsNotNull(wheel);

        Array2D<WheelSegmentTransitions> transitionDatas = new(_sidesRingsCount);

        for (int side = 0; side < _sidesRingsCount.x; side++)
        {
            QS_Transition vertOrderTransition = qst_data.TransitionsBuffer.GetQSTransition(side);
            Assert.IsTrue(vertOrderTransition.IsCreated);
            vertOrderTransition.GetSubTransition(new int2(0, _transSegsCounts.x / 2), out QS_Transition down);
            vertOrderTransition.GetSubTransition(new int2(_transSegsCounts.x / 2, _transSegsCounts.x / 2), out QS_Transition up);

            int2 downIndexer = new(0, 2);
            int2 upIndexer = new(0, 2);
            for (int ring = 0; ring < _sidesRingsCount.y; ring++)
            {
                WheelSegmentTransitions transData = new();
                transData.Down = down.GetSubTransition(downIndexer);
                transData.Up = up.GetSubTransition(upIndexer);
                transitionDatas[side, ring] = transData;
                Assert.IsTrue(transitionDatas[side, ring].Down.IsCreated && transitionDatas[side, ring].Up.IsCreated);

                MoveBufferIndexer(ref upIndexer, 2);
                MoveBufferIndexer(ref downIndexer, 2);
            }
        }

        for (int ring = 0; ring < _sidesRingsCount.y; ring++)
        {
            QS_Transition clockOrderTransition = qst_data.TransitionsBuffer.GetQSTransition(ring + _sidesRingsCount.x);
            Assert.IsTrue(clockOrderTransition.IsCreated);
            QS_Transition antiCw = clockOrderTransition.GetSubTransition(new int2(0, _transSegsCounts.y / 2));
            QS_Transition cw = clockOrderTransition.GetSubTransition(new int2(_transSegsCounts.y / 2, _transSegsCounts.y / 2));

            int2 antiCWIndexer = new int2(0, 2);
            int2 cwIndexer = new int2(0, 2);
            for (int side = 0; side < _sidesRingsCount.x; side++)
            {
                WheelSegmentTransitions transData = transitionDatas[side, ring];
                transData.AntiCW = antiCw.GetSubTransition(antiCWIndexer);
                transData.CW = cw.GetSubTransition(cwIndexer);

                transitionDatas[side, ring] = transData;
                Assert.IsTrue(transitionDatas[side, ring].AntiCW.IsCreated && transitionDatas[side, ring].CW.IsCreated);
                
                MoveBufferIndexer(ref cwIndexer, 2);
                MoveBufferIndexer(ref antiCWIndexer, 2);
            }
        }

        wheel.AssignTransitionDatas(transitionDatas);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}