using System;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

/// <summary>
/// Unity's Jobs system.
/// Fixed Collection size, which needs constructed buffers.
/// It is a segmented buffer in other words.
/// Indexers contain information about start and count of line segment.
/// </summary>
public struct QuadStripsBuffer : IDisposable
{
    private NativeArray<float3x2> _lineSegmentsBuffer;
    private NativeArray<int2> _quadStripsIndexersBuffer;
    public int LineSegmentsBufferLength { get { return _lineSegmentsBuffer.Length; } }
    public int QuadStripsCount { get { return _quadStripsIndexersBuffer.Length; } }
    public int2 Dims { get; set; }

    public QuadStripsBuffer(in NativeArray<float3x2> lineSegmentsBuffer, in NativeArray<int2> quadStripsIndexersBuffer)
    {
        _lineSegmentsBuffer = lineSegmentsBuffer;
        _quadStripsIndexersBuffer = quadStripsIndexersBuffer;
        Dims = int2.zero;
    }

    public int GetQuadCount()
    {
        return LineSegmentsBufferLength - QuadStripsCount;
    }

    public NativeArray<float3x2> GetBufferSegmentAndWriteIndexer(int2 indexer, int indexerIndex)
    {
        Assert.IsTrue(indexerIndex < QuadStripsCount);
        _quadStripsIndexersBuffer[indexerIndex] = indexer;
        return _lineSegmentsBuffer.GetSubArray(indexer.x, indexer.y);
    }

    public NativeArray<float3x2> GetBufferSegment(int index)
    { 
        Assert.IsTrue(index < QuadStripsCount);
        int2 indexer = _quadStripsIndexersBuffer[index];
        return _lineSegmentsBuffer.GetSubArray(indexer.x, indexer.y);
    }

    public QuadStrip GetQuadStrip(int index)
    {
        Assert.IsTrue(index < QuadStripsCount);
        int2 indexer = _quadStripsIndexersBuffer[index];
        NativeArray<float3x2> localLineSegments = _lineSegmentsBuffer.GetSubArray(indexer.x, indexer.y);
        return new QuadStrip(localLineSegments);
    }

    public int2 GetIndexer(int index)
    {
        Assert.IsTrue(index < QuadStripsCount);
        return _quadStripsIndexersBuffer[index];
    }

    public void Dispose()
    {
        _lineSegmentsBuffer.Dispose();
        _quadStripsIndexersBuffer.Dispose();
    }

    public void DisposeIfNeeded()
    {
        if (_lineSegmentsBuffer.IsCreated)
        {
            _lineSegmentsBuffer.Dispose();
        }

        if (_quadStripsIndexersBuffer.IsCreated)
        {
            _quadStripsIndexersBuffer.Dispose();
        }
    }
}