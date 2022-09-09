using Unity.Mathematics;
using Unity.Collections;
using UnityEngine.Assertions;

using Orazum.Collections;
using static Orazum.Math.LineSegmentUtilities;

// QS - quad strip
public struct QS_Transition
{
    private NativeArray<QST_Segment> _transSegs;
    public QS_Transition(NativeArray<QST_Segment> persistentAllocation)
    {
        _transSegs = persistentAllocation;
    }

    public int Length { get { return _transSegs.Length; } }
    public QST_Segment this[int index]
    {
        get
        {
            return _transSegs[index];
        }
    }
    public bool IsCreated { get { return _transSegs.IsCreated; } }

    public void GetSubTransition(int2 indexer, out QS_Transition subTransition)
    {
        subTransition = new QS_Transition(_transSegs.GetSubArray(indexer.x, indexer.y));
    }

    // In current setup transitions are disposed only if they are concatenated
    public void DisposeConcatenation()
    {
        _transSegs.Dispose();
    }

    public void DisposeConcatenationIfNeeded()
    {
        CollectionUtilities.DisposeIfNeeded(_transSegs);
    }

    public void DrawDebugRays(float duration)
    {
        for (int i = 0; i < _transSegs.Length; i++)
        {
            QST_Segment seg = _transSegs[i];
            DrawLineSegmentWithRaysUp(seg.StartLineSegment, 1, duration);
            DrawLineSegmentWithRaysUp(seg.EndLineSegment, 1, duration);
        }
    }

    public static QS_Transition Concatenate(
        QS_Transition t1, 
        QS_Transition t2, 
        NativeArray<QST_Segment> buffer
    )
    {
        Assert.IsTrue(buffer.Length == t1.Length + t2.Length);
        int bufferIndexer = 0;
        for (int i = 0; i < t1.Length; i++)
        {
            buffer[bufferIndexer++] = t1[i];
        }

        for (int i = 0; i < t2.Length; i++)
        {
            buffer[bufferIndexer++] = t2[i];
        }
        return new QS_Transition(buffer);
    }

    public static NativeArray<QST_Segment> PrepareConcatenationBuffer(
        QS_Transition t1,
        QS_Transition t2,
        Allocator allocator
    )
    {
        NativeArray<QST_Segment> buffer = new NativeArray<QST_Segment>(t1.Length + t2.Length, allocator);
        return buffer;
    }

    public override string ToString()
    {
        return $"{Length} transition segments\n";
    }
}