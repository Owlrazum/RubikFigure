using Unity.Mathematics;
using Orazum.Meshing;

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
    }

    public FigureSegment CompletionSegment { get; private set; }
    public void AssignCompletionSegment(FigureSegment completionSegment)
    {
        CompletionSegment = completionSegment;
    }

    private QS_Transition _transition;
    public ref QS_Transition Transition { get { return ref _transition; } }

    public bool ShouldReorientVertices { get; set; }
    public bool ShouldDisposeTransition { get; set; }
}
