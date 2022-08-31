using Unity.Mathematics;
using Unity.Collections;

public class FigureVerticesMove : FigureSegmentMove
{
    public FigureVerticesMove()
    {
        FromIndex = new int2(-1, -1);
        ToIndex = new int2(-1, -1);
    }

    public FigureVerticesMove(FigureSegmentMove move)
    {
        FromIndex = move.FromIndex;
        ToIndex = move.ToIndex;
        LerpSpeed = move.LerpSpeed;
        Mover = move.Mover;
    }

    public NativeArray<QSTransSegment>.ReadOnly TransSegments { get; private set; }
    public void AssignTransSegs(NativeArray<QSTransSegment>.ReadOnly transSegments)
    {
        TransSegments = transSegments;
    }
}
