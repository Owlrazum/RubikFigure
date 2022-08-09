public class IdleState : WheelState
{
    private SwipeCommand _currentSwipeCommand;
    public IdleState(WheelGenerationData generationData) : base(generationData)
    { 
        WheelStatesDelegates.IdleState += GetThisState;
    }

    public override void OnEnter(Wheel notUsed)
    {
        InputDelegatesContainer.SwipeCommand += OnSwipeCommand;
    }

    public override void OnExit()
    {
        InputDelegatesContainer.SwipeCommand -= OnSwipeCommand;
    }

    private void OnSwipeCommand(SwipeCommand swipeCommand)
    {
        _currentSwipeCommand = swipeCommand;
    }

    public override WheelState HandleTransitions()
    {
        if (_currentSwipeCommand == null)
        {
            return null;
        }
        else
        {
            MoveState moveState = WheelStatesDelegates.MoveState() as MoveState;
            moveState.AssignSwipeCommand(_currentSwipeCommand);
            _currentSwipeCommand = null;
            return moveState;
        }
    }

    public override void StartProcessingState(Wheel wheel)
    {

    }

    public override void OnDestroy()
    {
        WheelStatesDelegates.IdleState -= GetThisState;
        if (InputDelegatesContainer.SwipeCommand != null)
        { 
            InputDelegatesContainer.SwipeCommand -= OnSwipeCommand;
        }
    }

    protected override WheelState GetThisState()
    {
        return this;
    }
}