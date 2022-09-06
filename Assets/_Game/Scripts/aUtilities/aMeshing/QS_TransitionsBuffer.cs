using System;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

/// <summary>
/// Unity's Jobs system.
/// Fixed Collection size, which needs constructed buffers.
/// It is a segmented buffer in other words.
/// Indexers contain information about start and count of QuadStripTransitionSegment.
/// </summary>
public struct QS_TransitionsBuffer
{ 
    private NativeArray<QST_Segment> _qsTransSegmentsBuffer;
    private NativeArray<int2> _qsTransSegsIndexersBuffer;
    public int TransitionsCount { get { return _qsTransSegsIndexersBuffer.Length; } }

    public QS_TransitionsBuffer(NativeArray<QST_Segment> qsTransSegmentsBuffer, NativeArray<int2> qsTransSegsIndexersBuffer)
    {
        _qsTransSegmentsBuffer = qsTransSegmentsBuffer;
        _qsTransSegsIndexersBuffer = qsTransSegsIndexersBuffer;
    }

    public NativeArray<QST_Segment> GetBufferSegmentAndWriteIndexer(int2 indexer, int indexerIndex)
    {
        Assert.IsTrue(indexerIndex < TransitionsCount);
        _qsTransSegsIndexersBuffer[indexerIndex] = indexer;
        return _qsTransSegmentsBuffer.GetSubArray(indexer.x, indexer.y);
    }

    public NativeArray<QST_Segment> GetBufferSegment(int indexerIndex)
    {
        Assert.IsTrue(indexerIndex < TransitionsCount);
        int2 indexer = _qsTransSegsIndexersBuffer[indexerIndex];
        return _qsTransSegmentsBuffer.GetSubArray(indexer.x, indexer.y);
    }

    public QS_Transition GetQSTransition(int index)
    {
        Assert.IsTrue(index < TransitionsCount);
        int2 indexer = _qsTransSegsIndexersBuffer[index];
        NativeArray<QST_Segment> localLineSegments = _qsTransSegmentsBuffer.GetSubArray(indexer.x, indexer.y);
        return new QS_Transition(localLineSegments);
    }

    public int2 GetIndexer(int index)
    {
        Assert.IsTrue(index < TransitionsCount);
        return _qsTransSegsIndexersBuffer[index];
    }

    public void Dispose()
    { 
        _qsTransSegmentsBuffer.Dispose();
        _qsTransSegsIndexersBuffer.Dispose();
    }

    public void DisposeIfNeeded()
    {
        if (_qsTransSegmentsBuffer.IsCreated)
        {
            _qsTransSegmentsBuffer.Dispose();
        }

        if (_qsTransSegsIndexersBuffer.IsCreated)
        {
            _qsTransSegsIndexersBuffer.Dispose();
        }
    }
}