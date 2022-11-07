using UnityEngine;
using UnityEngine.Assertions;

public class FigureIdleState : FigureState
{
    protected SwipeCommand _currentSwipeCommand;
    protected FS_Point _currentSelectedPoint;
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

    protected virtual void SelectAction(Collider collider)
    {
        bool isFound = collider.TryGetComponent(out FSP_Collider segmentPointCollider);
        Assert.IsTrue(isFound);
        if (segmentPointCollider.ParentPoint.Segment == null)
        {
            _currentSelectedPoint = null;
            Debug.LogWarning("NoSegmentLocatedHere");
            return;
        }

        Debug.Log($"Select action {segmentPointCollider.ParentPoint.Index}");
        _currentSelectedPoint = segmentPointCollider.ParentPoint;
        _currentSelectedPoint.Segment.HighlightRender();
    }

    protected virtual bool DeselectCheckOnPointerUp(Collider collider)
    {
        return _currentSwipeCommand == null;
    }

    protected virtual void DeselectAction()
    {
        if (_currentSelectedPoint == null)
        {
            return;
        }

        Debug.Log("DeselectAction");
        _currentSelectedPoint.Segment.DefaultRender();
        _currentSelectedPoint = null;
    }

    protected virtual void OnSwipeCommand(SwipeCommand swipeCommand)
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