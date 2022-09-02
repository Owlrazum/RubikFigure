using System;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

/// <summary>
/// Unity's Jobs system.
/// Fixed Collection size, which needs constructed buffers.
/// Indexers contain information about start and count of int2.
/// </summary>
public struct SegmentedBufferInt2
{ 
    private NativeArray<int2> _int2Buffer;
    private NativeArray<int2> _int2IndexersBuffer;
    public int Int2Count { get { return _int2IndexersBuffer.Length; } }

    public SegmentedBufferInt2(NativeArray<int2> int2Buffer, NativeArray<int2> int2IndexersBuffer)
    {
        _int2Buffer = int2Buffer;
        _int2IndexersBuffer = int2IndexersBuffer;
    }

    public NativeArray<int2> GetBufferSegmentAndWriteIndexer(int2 indexer, int indexerIndex)
    {
        Assert.IsTrue(indexerIndex < Int2Count);
        _int2IndexersBuffer[indexerIndex] = indexer;
        return _int2Buffer.GetSubArray(indexer.x, indexer.y);
    }

    public NativeArray<int2> GetBufferSegment(int indexerIndex)
    {
        Assert.IsTrue(indexerIndex < Int2Count);
        int2 indexer = _int2IndexersBuffer[indexerIndex];
        return _int2Buffer.GetSubArray(indexer.x, indexer.y);
    }

    public int2 GetIndexer(int index)
    {
        Assert.IsTrue(index < Int2Count);
        return _int2IndexersBuffer[index];
    }

    public void Dispose()
    { 
        _int2Buffer.Dispose();
        _int2IndexersBuffer.Dispose();
    }

    public void DisposeIfNeeded()
    {
        if (_int2Buffer.IsCreated)
        {
            _int2Buffer.Dispose();
        }

        if (_int2IndexersBuffer.IsCreated)
        {
            _int2IndexersBuffer.Dispose();
        }
    }
}