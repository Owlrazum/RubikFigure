using Unity.Collections;
using UnityEngine.Assertions;

// QS - quad strip
public struct QS_Transition
{
    private NativeArray<QST_Segment>.ReadOnly _transSegs;
    public QS_Transition(NativeArray<QST_Segment> persistentAllocation)
    {
        _transSegs = persistentAllocation.AsReadOnly();
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
}