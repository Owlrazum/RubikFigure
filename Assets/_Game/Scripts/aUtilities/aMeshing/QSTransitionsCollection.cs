using System;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

/// <summary>
/// Unity's Jobs system.
/// Fixed Collection size, which needs constructed buffers.
/// Indexers contain information about start and count of QuadStripTransitionSegment.
/// </summary>
public struct QSTransitionsCollection
{ 
    private NativeArray<QSTransSegment> _qsTransSegmentsBuffer;
    private NativeArray<int2> _qsTransSegsIndexersBuffer;
    public int QSTransSegsCount { get { return _qsTransSegsIndexersBuffer.Length; } }

    public QSTransitionsCollection(NativeArray<QSTransSegment> qsTransSegments, NativeArray<int2> qsTransSegsIndexers)
    {
        _qsTransSegmentsBuffer = qsTransSegments;
        _qsTransSegsIndexersBuffer = qsTransSegsIndexers;
    }

    public NativeArray<QSTransSegment> GetWriteBufferAndWriteIndexer(int2 indexer, int indexerIndex)
    {
        Assert.IsTrue(indexerIndex < QSTransSegsCount);
        _qsTransSegsIndexersBuffer[indexerIndex] = indexer;
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