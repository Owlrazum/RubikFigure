using UnityEngine;
using UnityEngine.Assertions;

public class FigureIdleState : FigureState
{ 
    protected SwipeCommand _currentSwipeCommand;
    protected FigureSegmentPoint _currentSelectedPoint;
    protected Selectable _selectable;
    public FigureIdleState(Selectable selectable, FigureStatesController statesController, Figure figure) 
        : base(statesController, figure)
    {
        _selectable = selectable;
        _selectable.SetSelectionActions(SelectAction, DeselectCheckOnPointerUp, DeselectAction);
    }

    public override void OnEnter()
    {
        InputDelegatesContainer.RegisterSelectable(_selectable);
        InputDelegatesContainer.SetShouldRespond(true);

        InputDelegatesContainer.SwipeCommand += OnSwipeCommand;
    }

    public override void OnExit()
    {
        InputDelegatesContainer.UnregisterSelectable(_selectable);
        InputDelegatesContainer.SetShouldRespond(false);

        InputDelegatesContainer.SwipeCommand -= OnSwipeCommand;
    }

    private void SelectAction(Collider collider)
    {
        bool isFound = collider.TryGetComponent(out FigureSegmentPointCollider segmentPointCollider);
        Assert.IsTrue(isFound);
        if (segmentPointCollider.ParentPoint.Segment == null)
        {
            _currentSelectedPoint = null;
            Debug.LogWarning("NoSegmentLocatedHere");
            return;
        }

        _currentSelectedPoint = segmentPointCollider.ParentPoint;
        _currentSelectedPoint.Segment.HighlightRender();
    }

    private bool DeselectCheckOnPointerUp(Collider collider)
    {
        return _currentSwipeCommand == null;
        // if (collider == null)
        // {
        //     return true;
        // }
        
        // bool isFound = collider.TryGetComponent(out FigureSegmentPointCollider segmentPointCollider);
        // Assert.IsTrue(isFound);
        // if (segmentPointCollider.ParentPoint == _currentSelectedPoint)
        // {
        //     return false;
        // }
        // else
        // { 
        //     return true;
        // }
    }

    private void DeselectAction()
    {
        Debug.Log("DeselectAction");
        _currentSelectedPoint?.Segment.DefaultRender();
        _currentSelectedPoint = null;
    }

    private void OnSwipeCommand(SwipeCommand swipeCommand)
    {
        if (_currentSelectedPoint != null)
        { 
            _currentSwipeCommand = swipeCommand;
        }
    }

    public override FigureState HandleTransitions()
    {
        if (_currentSwipeCommand == null || _currentSelectedPoint == null)
        {
            return null;
        }
        else
        {
            FigureMoveState moveState = _statesController.MoveState;
            moveState.PrepareForMove(_currentSwipeCommand, _currentSelectedPoint);
            DeselectAction();
            _currentSwipeCommand = null;
            return moveState;
        }
    }

    public virtual void OnDestroy()
    {
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