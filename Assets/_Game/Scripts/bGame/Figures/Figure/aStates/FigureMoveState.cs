using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;

public abstract class FigureMoveState : FigureState
{ 
    protected float _moveLerpSpeed;
    protected bool _areSegmentsMoving;
    protected Vector3 _figureCenter;

    protected List<FigureSegmentMove> _movesToMake;

    public FigureMoveState(FigureStatesController statesController, Figure figure, float moveLerpSpeed) 
        : base(statesController, figure)
    {
        _moveLerpSpeed = moveLerpSpeed;
        _figureCenter = figure.transform.position;
    }

    protected abstract List<FigureSegmentMove> DetermineMovesFromInput(Vector3 worldPos, Vector3 worldDir);

    protected SwipeCommand _currentSwipeCommand;
    protected FigureSegmentPoint _currentSelectedPoint;
    public void PrepareForMove(SwipeCommand swipeCommand, FigureSegmentPoint selectedSegmentPoint)
    {
        _currentSwipeCommand = swipeCommand;
        _currentSelectedPoint = selectedSegmentPoint;
    }

    public override void OnEnter()
    {
        Assert.IsNotNull(_currentSwipeCommand);
        Assert.IsNotNull(_currentSelectedPoint.Segment);
        Camera renderingCamera = InputDelegatesContainer.GetRenderingCamera();
        float planeDistance = (_figureCenter - renderingCamera.transform.position).magnitude;
        Vector3 viewStartPos = new Vector3(_currentSwipeCommand.ViewStartPos.x,
            _currentSwipeCommand.ViewStartPos.y, planeDistance);
        Vector3 viewEndPos = new Vector3(_currentSwipeCommand.ViewEndPos.x,
            _currentSwipeCommand.ViewEndPos.y, planeDistance);

        Vector3 worldStartPos = renderingCamera.ViewportToWorldPoint(viewStartPos);
        Vector3 worldEndPos = renderingCamera.ViewportToWorldPoint(viewEndPos);
        Vector3 worldDir = (worldEndPos - worldStartPos).normalized;

        var moves = DetermineMovesFromInput(worldStartPos, worldDir);
        if (moves == null)
        {
            Debug.Log("no moves possible");
            _areSegmentsMoving = false;
        }
        else
        {
            Debug.Log("making moves");
            _areSegmentsMoving = true;
            _figure.MakeMoves(moves, OnMovesCompleted);
        }
    }

    protected void OnMovesCompleted()
    {
        _areSegmentsMoving = false;
        FigureDelegatesContainer.ActionCheckFigureCompletion(_figure);
    }

    public override FigureState HandleTransitions()
    {
        if (_areSegmentsMoving)
        {
            return null;
        }
        else
        { 
            return _statesController.IdleState;
        }
    }

    public override string ToString()
    {
        return "MoveState";
    }
}