using Unity.Mathematics;

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

    private QSTransition _transition;
    public ref QSTransition Transition { get { return ref _transition; } }

    public bool ShouldReorientVertices { get; set; }
}
