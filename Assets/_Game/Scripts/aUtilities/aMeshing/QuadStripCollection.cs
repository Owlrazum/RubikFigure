using System;
using Unity.Mathematics;
using Unity.Collections;

// for Unity's Job system
public struct QuadStripsCollection : IDisposable
{ 
    private NativeArray<float2x2> _lineSegments;
    private NativeArray<int2> _quadStripsIndexers;
    private int _quadStripsIndexersIndexer;

    public QuadStripsCollection(NativeArray<float2x2> lineSegments, NativeArray<int2> quadStripsIndexers)
    {
        _lineSegments = lineSegments;
        _quadStripsIndexers = quadStripsIndexers;
        _quadStripsIndexersIndexer = 0;
    }

    public void AddQuadStrip(NativeArray<float2x2> localLineSegments, int2 indexer)
    {
        int localLineSegmentsIndexer = 0;
        for (int i = indexer.x; i < indexer.x + indexer.y; i++)
        {
            _lineSegments[i] = localLineSegments[localLineSegmentsIndexer++];
        }
        _quadStripsIndexers[_quadStripsIndexersIndexer++] = indexer;
    }

    public QuadStrip GetTempQuadStrip(int index)
    {
        int2 indexer = _quadStripsIndexers[index];
        NativeArray<float2x2> localLineSegments = new NativeArray<float2x2>(indexer.y, Allocator.Temp);
        int localLineSegmentsIndexer = 0;
        for (int i = 0; i < indexer.y; i++)
        {
            localLineSegments[localLineSegmentsIndexer++] = _lineSegments[indexer.x + i];
        }
        return new QuadStrip(localLineSegments);
    }

    public QuadStrip[] AllocatePersistently()
    {
        QuadStrip[] quadStrips = new QuadStrip[_quadStripsIndexers.Length];
        int quadStripIndexer = 0;
        for (int i = 0; i < _quadStripsIndexers.Length; i++)
        {
            int2 indexer = _quadStripsIndexers[i];
            NativeArray<float2x2> localLineSegments = new NativeArray<float2x2>(indexer.y, Allocator.Persistent);
            int localIndexer = 0;
            for (int j = indexer.x; j < indexer.y; j++)
            {
                localLineSegments[localIndexer++] = _lineSegments[j];
            }
            QuadStrip quadStrip = new QuadStrip(localLineSegments);
            quadStrips[quadStripIndexer++] = quadStrip;
        }
        return quadStrips;
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