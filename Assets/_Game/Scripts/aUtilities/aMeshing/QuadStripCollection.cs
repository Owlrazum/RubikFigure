using System;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

/// <summary>
/// Unity's Jobs system.
/// Fixed Collection size, which needs constructed buffers.
/// Indexers contain information about start and count of line segment.
/// </summary>
public struct QuadStripsCollection : IDisposable
{
    private NativeArray<float3x2> _lineSegments;
    private NativeArray<int2> _quadStripsIndexers;
    public int QuadStripsCount { get { return _quadStripsIndexers.Length; } }

    public QuadStripsCollection(NativeArray<float3x2> lineSegments, NativeArray<int2> quadStripsIndexers)
    {
        _lineSegments = lineSegments;
        _quadStripsIndexers = quadStripsIndexers;
    }

    public NativeArray<float3x2> GetWriteBufferAndWriteIndexer(int2 indexer, int indexerIndex)
    {
        Assert.IsTrue(indexerIndex < QuadStripsCount);
        _quadStripsIndexers[indexerIndex] = indexer;
        return _lineSegments.GetSubArray(indexer.x, indexer.y);
    }

    public QuadStrip GetQuadStrip(int index)
    {
        Assert.IsTrue(index < QuadStripsCount);
        int2 indexer = _quadStripsIndexers[index];
        NativeArray<float3x2> localLineSegments = _lineSegments.GetSubArray(indexer.x, indexer.y);
        return new QuadStrip(localLineSegments);
    }

    public int2 GetIndexer(int index)
    {
        Assert.IsTrue(index < QuadStripsCount);
        return _quadStripsIndexers[index];
    }

    public int2 GetQuadIndexer(int index)
    {
        Assert.IsTrue(index < QuadStripsCount);
        int2 indexer = _quadStripsIndexers[index];
        indexer.x -= index;
        indexer.y -= 1;
        return indexer;
    }

    public void Dispose()
    {
        _lineSegments.Dispose();
        _quadStripsIndexers.Dispose();
    }

    public void DisposeIfNeeded()
    {
        if (_lineSegments.IsCreated)
        {
            _lineSegments.Dispose();
        }

        if (_quadStripsIndexers.IsCreated)
        {
            _quadStripsIndexers.Dispose();
        }
    }
}