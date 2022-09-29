using Orazum.Meshing;

public class FMS_Transition : FM_Segment
{ 
    private QS_Transition _transition;
    public ref QS_Transition Transition { get { return ref _transition; } }

    public bool ShouldDisposeTransition { get; set; }
}