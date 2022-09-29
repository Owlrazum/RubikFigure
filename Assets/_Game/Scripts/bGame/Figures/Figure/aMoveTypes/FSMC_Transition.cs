using Orazum.Meshing;

public class FSMC_Transition : FSM_IndexChange
{
    public FSMC_Transition() : base()
    {
    }

    public FSMC_Transition(FSM_IndexChange indexChange)
    {
        From = indexChange.From;
        To = indexChange.To;
        LerpSpeed = indexChange.LerpSpeed;
    }

    private QS_Transition _transition;
    public ref QS_Transition Transition { get { return ref _transition; } }

    public bool ShouldDisposeTransition { get; set; }
}
