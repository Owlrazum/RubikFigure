using Unity.Mathematics;
using Unity.Collections;

public class ValknutVerticesMove : FigureSegmentMove
{
    public ValknutVerticesMove()
    {
        FromIndex = new int2(-1, -1);
        ToIndex = new int2(-1, -1);
    }

    public ValknutVerticesMove(FigureSegmentMove move)
    {
        FromIndex = move.FromIndex;
        ToIndex = move.ToIndex;
        LerpSpeed = move.LerpSpeed;
        Mover = move.Mover;
    }

    public NativeArray<QSTransSegment>.ReadOnly TransitionData { get; private set; }
    public void AssignTransitionPositions(NativeArray<QSTransSegment>.ReadOnly transitionData)
    {
        TransitionData = transitionData;
    }
}
