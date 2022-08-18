public class WheelIdleState : FigureIdleState
{
    public WheelIdleState()
    {
        WheelDelegates.IdleState += GetThisState;
    }

    public override FigureState MoveToAnotherStateOnInput()
    {
        WheelMoveState moveState = WheelDelegates.MoveState();
        moveState.PrepareForMove(_currentSwipeCommand, _currentSelectedPoint);
        return WheelDelegates.MoveState();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        WheelDelegates.IdleState -= GetThisState;
    }

    private WheelIdleState GetThisState()
    {
        return this;
    }

    public override string ToString()
    {
        return "WheelIdleState";
    }
}