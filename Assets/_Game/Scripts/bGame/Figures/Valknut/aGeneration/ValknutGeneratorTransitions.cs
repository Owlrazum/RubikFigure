using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Unity.Mathematics;
using Orazum.Constants;
using Orazum.Meshing;
using Orazum.Collections;


public class ValknutGeneratorTransitions : FigureGeneratorTransitions
{
    private const int TotalRangesCount = (7 + 6) * 3 + (6 + 5) * 3;
    private const int TotalTransitionsCount = 2 * 3 + 2 * 3;

    private NativeArray<int2> _originTargetIndices;

    protected override void StartTransitionsGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency)
    { 
        _originTargetIndices = new NativeArray<int2>(TotalTransitionsCount, Allocator.Persistent);
        NativeArray<int2> bufferIndexers = new NativeArray<int2>(TotalTransitionsCount, Allocator.Persistent);
        GenerateDataJobIndexData(originTargetIndices: ref _originTargetIndices, buffersIndexers: ref bufferIndexers);
        
        NativeArray<QST_Segment> writeBuffer = new NativeArray<QST_Segment>(TotalRangesCount, Allocator.Persistent);
        _transitionsCollection = new QS_TransitionsBuffer(writeBuffer, bufferIndexers);

        ValknutGenJobTransData transitionDataJob = new ValknutGenJobTransData()
        {
            InQuadStripsCollection = quadStripsCollection,
            InOriginTargetIndices = _originTargetIndices,
            OutTransitionsCollection = _transitionsCollection
        };
        _dataJobHandle = transitionDataJob.ScheduleParallel(TotalTransitionsCount, 32, dependency);
    }

    private void GenerateDataJobIndexData(ref NativeArray<int2> originTargetIndices, ref NativeArray<int2> buffersIndexers)
    {
        int2 originIndices = new int2(4, 5);
        int targetIndex = 0;
        
        int bufferStart = 0;
        int2 rangesCount = new int2(7, 6);

        int originTargetIndicesIndexer = 0;
        int buffersIndexersIndexer = 0;
        for (int i = 0; i < 6; i += 2)
        {
            originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.x, targetIndex);
            buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.x);
            bufferStart += rangesCount.x;

            originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.y, targetIndex);
            buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.y);
            bufferStart += rangesCount.y;

            originIndices.x = originIndices.x + 2 >= 6 ? 0 : originIndices.x + 2;
            originIndices.y = originIndices.y + 2 >= 6 ? 1 : originIndices.y + 2;
            targetIndex += 2;
        }

        targetIndex = 1;
        originIndices = new int2(5, 4);
        rangesCount = new int2(5, 6);
        for (int i = 0; i < 6; i += 2)
        {
            originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.x, targetIndex);
            buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.x);
            bufferStart += rangesCount.x;

            originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.y, targetIndex);
            buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.y);
            bufferStart += rangesCount.y;

            originIndices.x = originIndices.x + 2 >= 6 ? 1 : originIndices.x + 2;
            originIndices.y = originIndices.y + 2 >= 6 ? 0 : originIndices.y + 2;
            targetIndex += 2;
        }
    }

    protected override void FinishTransitionsGeneration(Figure figure)
    {
        _dataJobHandle.Complete();

        Valknut valknut = figure as Valknut;
        Assert.IsNotNull(valknut);

        Array2D<ValknutSegmentTransitions> transitionDatas =
            new Array2D<ValknutSegmentTransitions>(new int2(Valknut.TrianglesCount, Valknut.PartsCount));
        for (int i = 0; i < _transitionsCollection.TransitionsCount; i += 2)
        {
            QS_Transition clockWiseTransition = _transitionsCollection.GetQSTransition(i);
            QS_Transition antiClockWiseTransition = _transitionsCollection.GetQSTransition(i + 1);

            ValknutSegmentTransitions transData = new ValknutSegmentTransitions();
            ValknutSegmentTransitions.Clockwise(ref transData) = clockWiseTransition;
            ValknutSegmentTransitions.AntiClockwise(ref transData) = antiClockWiseTransition;

            int2 originTargetIndex = _originTargetIndices[i];
            int2 segmentIndex = new int2(originTargetIndex.y / Valknut.PartsCount, originTargetIndex.y % Valknut.PartsCount);
            transitionDatas[segmentIndex] = transData;
        }

        valknut.AssignTransitionDatas(transitionDatas);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CollectionUtilities.DisposeIfNeeded(_originTargetIndices);
    }
}