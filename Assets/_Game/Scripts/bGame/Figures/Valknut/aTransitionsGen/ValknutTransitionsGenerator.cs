using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Unity.Mathematics;
using Orazum.Constants;
using Orazum.Meshing;
using Orazum.Collections;

using static Orazum.Meshing.BufferUtils;
using static Orazum.Collections.IndexUtilities;

public class ValknutTransitionsGenerator : FigureTransitionsGenerator
{
    private const int TotalRangesCount = (7 + 6) * 3 + (6 + 5) * 3;
    private const int TotalTransitionsCount = 2 * 3 + 2 * 3;

    protected override void StartTransitionsGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency)
    {
        PrepareNativeData();
        ValknutGenJobTransData transitionDataJob = new ValknutGenJobTransData()
        {
            InQuadStripsCollection = quadStripsCollection,
            OutTransitionsCollection = qst_data.TransitionsBuffer
        };
        _jobHandle = transitionDataJob.ScheduleParallel(TotalTransitionsCount, 32, dependency);
    }

    private void PrepareNativeData()
    {
        qst_data.IndexersBuffer = new NativeArray<int2>(TotalTransitionsCount, Allocator.Persistent);
        qst_data.SegmentsBuffer = new NativeArray<QST_Segment>(TotalRangesCount, Allocator.Persistent);
        qst_data.TransitionsBuffer = new QS_TransitionsBuffer(qst_data.SegmentsBuffer, qst_data.IndexersBuffer);

        int qsts_indexer = 0;
        int2 qst_bufferIndexer = int2.zero;
        int2 outerSegmentsTransSegsCount = new int2(7, 6);
        int2 innerSegmentsTransSegsCount = new int2(5, 6);

        for (int triangle = 0; triangle < Valknut.TrianglesCount; triangle++)
        {
            MoveBufferIndexer(ref qst_bufferIndexer, outerSegmentsTransSegsCount.x);
            qst_data.IndexersBuffer[qsts_indexer++] = qst_bufferIndexer;
            MoveBufferIndexer(ref qst_bufferIndexer, outerSegmentsTransSegsCount.y);
            qst_data.IndexersBuffer[qsts_indexer++] = qst_bufferIndexer;

            MoveBufferIndexer(ref qst_bufferIndexer, innerSegmentsTransSegsCount.x);
            qst_data.IndexersBuffer[qsts_indexer++] = qst_bufferIndexer;
            MoveBufferIndexer(ref qst_bufferIndexer, innerSegmentsTransSegsCount.y);
            qst_data.IndexersBuffer[qsts_indexer++] = qst_bufferIndexer;
        }
    }

    protected override void FinishTransitionsGeneration(Figure figure)
    {
        _jobHandle.Complete();

        Valknut valknut = figure as Valknut;
        Assert.IsNotNull(valknut);

        Array2D<ValknutSegmentTransitions> transitionDatas =
            new Array2D<ValknutSegmentTransitions>(new int2(Valknut.TrianglesCount, Valknut.PartsCount));

        int triangleIndexer = 0;
        for (int i = 0; i < qst_data.TransitionsBuffer.TransitionsCount; i += 4)
        {
            QS_Transition outerCW = qst_data.TransitionsBuffer.GetQSTransition(i);
            QS_Transition outerAntiCW = qst_data.TransitionsBuffer.GetQSTransition(i + 1);

            ValknutSegmentTransitions transData = new ValknutSegmentTransitions();
            transData.CW = outerCW;
            transData.AntiCW = outerAntiCW;

            int2 segmentIndex = new int2(triangleIndexer, 0);
            transitionDatas[segmentIndex] = transData;

            QS_Transition innerCW = qst_data.TransitionsBuffer.GetQSTransition(i + 2);
            QS_Transition innerAntiCW = qst_data.TransitionsBuffer.GetQSTransition(i + 3);

            transData = new ValknutSegmentTransitions();
            transData.CW = innerCW;
            transData.AntiCW = innerAntiCW;

            segmentIndex = new int2(triangleIndexer, 1);
            transitionDatas[segmentIndex] = transData;
            
            triangleIndexer++;
        }

        valknut.AssignTransitionDatas(transitionDatas);
    }
}