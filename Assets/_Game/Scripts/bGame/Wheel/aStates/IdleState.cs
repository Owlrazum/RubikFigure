using UnityEngine;
using UnityEngine.Assertions;

public class IdleState : WheelState
{
    private SwipeCommand _currentSwipeCommand;
    private SegmentPoint _currentSelectedPoint;

    public IdleState(LevelDescriptionSO levelDescription, Wheel wheelArg) : base(levelDescription, wheelArg)
    {
        WheelDelegates.IdleState += GetThisState;
    }

    public override void OnEnter()
    {
        InputDelegatesContainer.SelectSegmentCommand += OnSelectSegmentCommand;
        InputDelegatesContainer.DeselectSegmentCommand += OnDeselectSegmentCommand;
        InputDelegatesContainer.SwipeCommand += OnSwipeCommand;
        InputDelegatesContainer.SetShouldRespond(true);
    }

    public override void OnExit()
    {
        InputDelegatesContainer.SelectSegmentCommand -= OnSelectSegmentCommand;
        InputDelegatesContainer.DeselectSegmentCommand -= OnDeselectSegmentCommand;
        InputDelegatesContainer.SwipeCommand -= OnSwipeCommand;
        InputDelegatesContainer.SetShouldRespond(false);
    }

    private void OnSelectSegmentCommand(Collider segmentPointCollider)
    {
        bool isFound = segmentPointCollider.TryGetComponent(out SegmentPoint segmentPoint);
        Assert.IsTrue(isFound);
        if (segmentPoint.Segment == null)
        {
            _currentSelectedPoint = null;
            Debug.LogWarning("No segment located here");
            return;
        }

        _currentSelectedPoint = segmentPoint;
        _currentSelectedPoint.Segment.HighlightRender();
    }

    private void OnDeselectSegmentCommand()
    {
        _currentSelectedPoint?.Segment.DefaultRender();
        _currentSelectedPoint = null;
    }

    private void OnSwipeCommand(SwipeCommand swipeCommand)
    {
        Debug.Log("Swipe");
        _currentSwipeCommand = swipeCommand;
    }

    public override WheelState HandleTransitions()
    {
        if (_currentSwipeCommand == null || _currentSelectedPoint == null)
        {
            Debug.Log($"{_currentSwipeCommand == null} {_currentSelectedPoint == null}");
            return null;
        }
        else
        {
            MoveState moveState = WheelDelegates.MoveState() as MoveState;
            moveState.PrepareForMove(_currentSwipeCommand, _currentSelectedPoint);
            _currentSwipeCommand = null;
            OnDeselectSegmentCommand();
            return moveState;
        }
    }

    public override void ProcessState()
    {

    }

    public override void OnDestroy()
    {
        WheelDelegates.IdleState -= GetThisState;
        if (InputDelegatesContainer.SelectSegmentCommand == null)
        { 
            InputDelegatesContainer.SelectSegmentCommand -= OnSelectSegmentCommand;
        }
        if (InputDelegatesContainer.DeselectSegmentCommand == null)
        { 
            InputDelegatesContainer.DeselectSegmentCommand -= OnDeselectSegmentCommand;
        }
        if (InputDelegatesContainer.SwipeCommand != null)
        { 
            InputDelegatesContainer.SwipeCommand -= OnSwipeCommand;
        }
    }

    protected override WheelState GetThisState()
    {
        return this;
    }

    public override string ToString()
    {
        return "IdleState";
    }
}