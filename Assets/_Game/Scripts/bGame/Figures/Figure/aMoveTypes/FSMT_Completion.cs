using Unity.Mathematics;

public class FSMT_Completion : FSM_Transition
{
    public int2 CompletionIndex { get; private set; }
    public void AssignCompletionIndex(int2 index)
    {
        CompletionIndex = index;
    }

    public FigureSegment CompletionSegment { get; private set; }
    public void AssignCompletionSegment(FigureSegment completionSegment)
    {
        CompletionSegment = completionSegment;
    }
}
