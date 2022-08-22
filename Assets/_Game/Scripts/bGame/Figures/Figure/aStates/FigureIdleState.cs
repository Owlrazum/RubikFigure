using UnityEngine;
using UnityEngine.Assertions;

public abstract class FigureIdleState : FigureState
{ 
    protected SwipeCommand _currentSwipeCommand;
    protected FigureSegmentPoint _currentSelectedPoint;

    public abstract FigureState MoveToAnotherStateOnInput();

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

    private void OnSelectSegmentCommand(Collider collider)
    {
        Debug.Log("OnSelectedSegmentCommand");
        bool isFound = collider.TryGetComponent(out FigureSegmentPointCollider segmentPointCollider);
        Assert.IsTrue(isFound);
        if (segmentPointCollider.ParentPoint.Segment == null)
        {
            _currentSelectedPoint = null;
            Debug.LogWarning("No segment located here");
            return;
        }

        _currentSelectedPoint = segmentPointCollider.ParentPoint;
        _currentSelectedPoint.Segment.HighlightRender();
    }

    private void OnDeselectSegmentCommand()
    {
        _currentSelectedPoint?.Segment.DefaultRender();
        _currentSelectedPoint = null;
    }

    private void OnSwipeCommand(SwipeCommand swipeCommand)
    {
        _currentSwipeCommand = swipeCommand;
    }

    public override FigureState HandleTransitions()
    {
        if (_currentSwipeCommand == null || _currentSelectedPoint == null)
        {
            return null;
        }
        else
        {
            FigureState anotherState = MoveToAnotherStateOnInput();
            Assert.IsNotNull(anotherState);
            _currentSwipeCommand = null;
            OnDeselectSegmentCommand();
            return anotherState;
        }
    }

    public override void OnDestroy()
    {
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

    public override string ToString()
    {
        return "IdleState";
    }
}