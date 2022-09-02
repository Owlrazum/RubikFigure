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
public struct QSTransitionsBuffer
{ 
    private NativeArray<QSTransSegment> _qsTransSegmentsBuffer;
    private NativeArray<int2> _qsTransSegsIndexersBuffer;
    public int QSTransSegsCount { get { return _qsTransSegsIndexersBuffer.Length; } }

    public QSTransitionsBuffer(NativeArray<QSTransSegment> qsTransSegmentsBuffer, NativeArray<int2> qsTransSegsIndexersBuffer)
    {
        _qsTransSegmentsBuffer = qsTransSegmentsBuffer;
        _qsTransSegsIndexersBuffer = qsTransSegsIndexersBuffer;
    }

    public NativeArray<QSTransSegment> GetBufferSegmentAndWriteIndexer(int2 indexer, int indexerIndex)
    {
        Assert.IsTrue(indexerIndex < QSTransSegsCount);
        _qsTransSegsIndexersBuffer[indexerIndex] = indexer;
        return _qsTransSegmentsBuffer.GetSubArray(indexer.x, indexer.y);
    }

    public NativeArray<QSTransSegment> GetBufferSegment(int indexerIndex)
    {
        Assert.IsTrue(indexerIndex < QSTransSegsCount);
        int2 indexer = _qsTransSegsIndexersBuffer[indexerIndex];
        return _qsTransSegmentsBuffer.GetSubArray(indexer.x, indexer.y);
    }

    public QSTransition GetQSTransition(int index)
    {
        Assert.IsTrue(index < QSTransSegsCount);
        int2 indexer = _qsTransSegsIndexersBuffer[index];
        NativeArray<QSTransSegment> localLineSegments = _qsTransSegmentsBuffer.GetSubArray(indexer.x, indexer.y);
        return new QSTransition(localLineSegments);
    }

    public int2 GetIndexer(int index)
    {
        Assert.IsTrue(index < QSTransSegsCount);
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