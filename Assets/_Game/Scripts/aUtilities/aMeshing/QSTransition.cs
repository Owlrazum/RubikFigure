using Unity.Collections;
using UnityEngine.Assertions;

// QS - quad strip
public struct QSTransition
{
    private NativeArray<QSTransSegment>.ReadOnly _transSegs;
    public QSTransition(NativeArray<QSTransSegment> persistentAllocation)
    {
        _transSegs = persistentAllocation.AsReadOnly();
    }

    public int Length { get { return _transSegs.Length; } }
    public QSTransSegment this[int index]
    {
        get
        {
            return _transSegs[index];
        }
    }
    public bool IsCreated { get { return _transSegs.IsCreated; } }

    public static QSTransition Concatenate(
        QSTransition t1, 
        QSTransition t2, 
        NativeArray<QSTransSegment> buffer
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
        return new QSTransition(buffer);
    }

    public static NativeArray<QSTransSegment> PrepareConcatenationBuffer(
        QSTransition t1,
        QSTransition t2,
        Allocator allocator
    )
    {
        NativeArray<QSTransSegment> buffer = new NativeArray<QSTransSegment>(t1.Length + t2.Length, allocator);
        return buffer;
    }
}