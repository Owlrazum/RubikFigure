using Orazum.Meshing;

public class FSM_Transition : FS_Movement
{ 
    private QS_Transition _transition;
    public ref QS_Transition Transition { get { return ref _transition; } }

    public bool ShouldDisposeTransition { get; set; }
}