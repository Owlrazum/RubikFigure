using UnityEngine;
using UnityEngine.Assertions;

public class IdleState : WheelState
{
    private SwipeCommand _currentSwipeCommand;
    private SegmentPoint _currentSelectedPoint;
    private bool _isShuffleCommandTriggered;

    public IdleState(LevelDescriptionSO levelDescription, Wheel wheelArg) : base(levelDescription, wheelArg)
    {
        WheelStatesDelegates.IdleState += GetThisState;
    }

    public override void OnEnter()
    {
        InputDelegatesContainer.SelectSegmentCommand += OnSelectSegmentCommand;
        InputDelegatesContainer.DeselectSegmentCommand += OnDeselectSegmentCommand;
        InputDelegatesContainer.SwipeCommand += OnSwipeCommand;
        InputDelegatesContainer.ShuffleCommand += OnShuffleCommand;
        InputDelegatesContainer.SetShouldRespond(true);
    }

    public override void OnExit()
    {
        InputDelegatesContainer.SelectSegmentCommand -= OnSelectSegmentCommand;
        InputDelegatesContainer.DeselectSegmentCommand -= OnDeselectSegmentCommand;
        InputDelegatesContainer.SwipeCommand -= OnSwipeCommand;
        InputDelegatesContainer.ShuffleCommand -= OnShuffleCommand;
        InputDelegatesContainer.SetShouldRespond(false);
    }

    private void OnSelectSegmentCommand(Collider segmentPointCollider)
    {
        bool isFound = segmentPointCollider.TryGetComponent(out SegmentPoint segmentPoint);
        Assert.IsTrue(isFound);
        if (segmentPoint.Segment == null)
        {
            return;
        }

        if (!_wheel.DoesIndexHaveAdjacentEmptyIndex(segmentPoint.Index))
        {
            return;
        }

        _currentSelectedPoint = segmentPoint;
        _currentSelectedPoint.Segment.HighlightRender();
    }

    private void OnDeselectSegmentCommand()
    {
        _currentSelectedPoint.Segment.DefaultRender();
    }

    private void OnSwipeCommand(SwipeCommand swipeCommand)
    {
        _currentSwipeCommand = swipeCommand;
    }

    private void OnShuffleCommand()
    {
        _isShuffleCommandTriggered = true;
    }

    public override WheelState HandleTransitions()
    {
        if (_isShuffleCommandTriggered)
        {
            // _isShuffleCommandTriggered = false;
            // ShuffleState shuffleState = WheelStatesDelegates.ShuffleState() as ShuffleState;
            // _wheel.Get
            // shuffleState.PrepareForShuffle();
            // return shuffleState;
        }
        if (_currentSwipeCommand == null)
        {
            return null;
        }
        else
        {
            MoveState moveState = WheelStatesDelegates.MoveState() as MoveState;
            Assert.IsTrue(_currentSwipeCommand != null && _currentSelectedPoint != null);
            moveState.PrepareForMove(_currentSwipeCommand, _currentSelectedPoint);
            _currentSwipeCommand = null;
            return moveState;
        }
    }

    public override void ProcessState()
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