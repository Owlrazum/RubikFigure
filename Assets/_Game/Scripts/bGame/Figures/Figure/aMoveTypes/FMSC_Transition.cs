using Orazum.Meshing;

public class FMSC_Transition : FMS_IndexChange
{
    public FMSC_Transition() : base()
    {
    }

    public FMSC_Transition(FMS_IndexChange indexChange)
    {
        From = indexChange.From;
        To = indexChange.To;
        LerpSpeed = indexChange.LerpSpeed;
    }

    private QS_Transition _transition;
    public ref QS_Transition Transition { get { return ref _transition; } }

    public bool ShouldDisposeTransition { get; set; }
}
