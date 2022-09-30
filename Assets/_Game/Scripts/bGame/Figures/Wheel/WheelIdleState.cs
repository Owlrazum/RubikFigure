using System;
using System.Collections.Generic;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

public class WheelIdleState : FigureIdleState
{
    private float _scaleDelta;
    private float _lerpSpeed;
    private WheelSegmentScaler _scaler;

    private enum State
    {
        Down,
        ToUp,
        Up,
        ToDown,
        Move
    }
    private State _state;

    private bool _isSelected;
    private bool _isMoveCommmanded;
    private bool _isCurrentScaleMoveCompleted;

    public WheelIdleState(Selectable selectable, FigureStatesController statesController, Figure figure)
        : base(selectable, statesController, figure)
    {
        _isCurrentScaleMoveCompleted = true;
    }

    public void AssignScalingParams(float lerpSpeed, float scaling, WheelGenParamsSO genParams)
    {
        _lerpSpeed = lerpSpeed;
        _scaleDelta = scaling;

        Assert.IsNotNull(genParams);
        _scaler = new WheelSegmentScaler();
        _scaler.AssignGenParams(genParams);
    }

    protected override void SelectAction(Collider collider)
    {
        if (_state != State.Down)
        {
            return;
        }

        bool isFound = collider.TryGetComponent(out FigureSegmentPointCollider segmentPointCollider);
        Assert.IsTrue(isFound);
        if (segmentPointCollider.ParentPoint.Segment == null)
        {
            _currentSelectedPoint = null;
            Debug.LogWarning("NoSegmentLocatedHere");
            return;
        }

        _isSelected = true;
        _currentSelectedPoint = segmentPointCollider.ParentPoint;
    }

    protected override bool DeselectCheckOnPointerUp(Collider collider)
    {
        return _isSelected;
    }

    protected override void DeselectAction()
    {
        _isSelected = false;
    }

    private void OnScaleMoveCompleted()
    {
        _isCurrentScaleMoveCompleted = true;
    }

    public override FigureState HandleTransitions()
    {
        if (_isMoveCommmanded && _state == State.Down)
        {
            _isMoveCommmanded = false;
            _currentSelectedPoint = null;
            return _statesController.MoveState;
        }
        else
        {
            return null;
        }
    }

    protected override void OnSwipeCommand(SwipeCommand swipeCommand)
    {
        if (!_isMoveCommmanded)
        {
            Debug.Log("Assigned swipe command");
            _currentSwipeCommand = swipeCommand;
        }
    }

    public override void ProcessState()
    {
        if (_isCurrentScaleMoveCompleted)
        {
            CheckSwipeCommand();
            if (_isSelected)
            {
                if (_state == State.Down)
                { 
                    _state = State.ToUp;
                    _isCurrentScaleMoveCompleted = false;
                    ScaleUp();
                    return;
                }
            }
            else
            {
                if (_state == State.Up)
                {
                    _state = State.ToDown;
                    _isCurrentScaleMoveCompleted = false;
                    ScaleDown();
                    return;
                }
            }

            if (_state == State.ToDown)
            {
                _state = State.Down;
            }
            else if (_state == State.ToUp)
            {
                _state = State.Up;
            }
        }
    }

    private void CheckSwipeCommand()
    {
        if (_currentSwipeCommand != null && !_isMoveCommmanded)
        {
            FigureMoveState moveState = _statesController.MoveState;
            moveState.PrepareForMove(_currentSwipeCommand, _currentSelectedPoint);
            _isMoveCommmanded = true;
            _currentSwipeCommand = null;
        }
    }

    private void ScaleUp()
    {
        Debug.Log("ScaleUp");
        Scale(new float2(1, 1 + _scaleDelta));
        _currentSelectedPoint.Segment.HighlightRender();
    }

    private void ScaleDown()
    {
        Debug.Log("ScaleDown");
        Scale(new float2(1 + _scaleDelta, 1));
        _currentSelectedPoint.Segment.DefaultRender();
        _currentSelectedPoint = null;
    }

    private void Scale(float2 originTargetScales)
    {
        FMS_Scaling scalingMove = new FMS_Scaling();
        _scaler.Setup(_currentSelectedPoint.Index, originTargetScales);
        scalingMove.AssignIndex(_currentSelectedPoint.Index);
        scalingMove.AssignLerpSpeed(_lerpSpeed);
        scalingMove.AssignScaler(_scaler);

        List<FigureMoveOnSegment> moves = new List<FigureMoveOnSegment>(1);
        moves.Add(scalingMove);
        Debug.Log("Starting scale move");
        _figure.MakeMoves(moves, OnScaleMoveCompleted);
    }
}