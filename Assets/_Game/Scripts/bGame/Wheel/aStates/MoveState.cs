using System.Collections.Generic;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using static Orazum.Math.MathUtilities;

public class MoveState : WheelState
{
    private SwipeCommand _currentSwipeCommand;
    private SegmentPoint _currentSelectedPoint;
    private Segment _segmentToMove;
    private VerticesMove _verticesMove; // we store it once to avoid gc. SegemntToMove presence determines logic.
    private RotationMove _rotationMove; // same as above.
    private float _moveLerpSpeed;

    public MoveState(LevelDescriptionSO levelDescription, Wheel wheelArg) : base(levelDescription, wheelArg)
    { 
        _moveLerpSpeed = levelDescription.MoveLerpSpeed;
        _verticesMove = new VerticesMove();
        _rotationMove = new RotationMove();

        WheelDelegates.MoveState += GetThisState;
    }

    public void PrepareForMove(SwipeCommand swipeCommand, SegmentPoint selectedSegmentPoint)
    {
        _currentSwipeCommand = swipeCommand;
        _currentSelectedPoint = selectedSegmentPoint;
    }

    public override void OnEnter()
    {
        Assert.IsNotNull(_currentSwipeCommand);
        Assert.IsNotNull(_currentSelectedPoint.Segment);

        Camera renderingCamera = GameDelegatesContainer.GetRenderingCamera();
        Vector3 center = _wheel.transform.position;
        float planeDistance = (center - renderingCamera.transform.position).magnitude;
        Vector3 viewStartPos = new Vector3(_currentSwipeCommand.ViewStartPos.x,
            _currentSwipeCommand.ViewStartPos.y, planeDistance);
        Vector3 viewEndPos = new Vector3(_currentSwipeCommand.ViewEndPos.x,
            _currentSwipeCommand.ViewEndPos.y, planeDistance);

        Vector3 worldStartPos = renderingCamera.ViewportToWorldPoint(viewStartPos);
        Vector3 worldEndPos = renderingCamera.ViewportToWorldPoint(viewEndPos);
        Vector3 worldDir = (worldEndPos - worldStartPos).normalized;

        SegmentMove _moveToMake = DetermineMoveFromInput(center, worldStartPos, worldDir);
        Debug.Log("Make Rotation moves " + _moveToMake);
        if (_moveToMake is RotationMove rotationMove)
        {
            MakeRotationMoves(_currentSelectedPoint.Index.y, rotationMove.Type);
            return;
        }
        else if (_moveToMake is VerticesMove verticesMove)
        {
            _moveToMake.AssignFromIndex(_currentSelectedPoint.Index);
            if (_wheel.IsMovePossibleFromIndex(verticesMove, out int2 toIndex))
            {
                _moveToMake.AssignToIndex(toIndex);
                _wheel.MakeVerticesMove(in verticesMove, _moveLerpSpeed, OnCurrentMoveCompleted);
                _segmentToMove = _currentSelectedPoint.Segment;
                return;
            }
        }
        
        _segmentToMove = null;
    }

    private SegmentMove DetermineMoveFromInput(Vector3 circleCenter, Vector3 worldPos, Vector3 worldDir)
    {
        Vector3 DirToCenter = (circleCenter - worldPos).normalized;
        float rotateAngleDeg = Mathf.Atan2(DirToCenter.z, DirToCenter.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(rotateAngleDeg, Vector3.up);
        worldDir = rotation * worldDir;
        float swipeAngle = Mathf.Atan2(worldDir.z, worldDir.x);
        if (swipeAngle > -TAU / 12 && swipeAngle < TAU / 12)
        {
            _verticesMove.AssignType(VerticesMove.TypeType.Down);
            return _verticesMove;
        }
        else if (swipeAngle > TAU / 12 && swipeAngle < 5 * TAU / 12)
        {
            _rotationMove.AssignType(RotationMove.TypeType.Clockwise);
            return _rotationMove;
        }
        else if (swipeAngle > -5 * TAU / 12 && swipeAngle < -TAU / 12)
        {
            _rotationMove.AssignType(RotationMove.TypeType.CounterClockwise);
            return _rotationMove;
        }
        else
        {
            _verticesMove.AssignType(VerticesMove.TypeType.Up);
            return _verticesMove;
        }
    }

    private void MakeRotationMoves(int ringIndex, RotationMove.TypeType type)
    {
        int2 index = new int2(0, ringIndex);
        int2 nextIndex = int2.zero; 
        if (type == RotationMove.TypeType.Clockwise)
        { 
            nextIndex = _wheel.MoveIndexClockwise(index);
        }
        else if (type == RotationMove.TypeType.CounterClockwise)
        { 
            nextIndex = _wheel.MoveIndexCounterClockwise(index);
        }
        List<RotationMove> rotationMoves = new List<RotationMove>(_wheel.SideCount);
        for (int i = 0; i < _wheel.SideCount; i++)
        {
            index = nextIndex;
            if (type == RotationMove.TypeType.Clockwise)
            {
                nextIndex = _wheel.MoveIndexClockwise(index);
            }
            else if (type == RotationMove.TypeType.CounterClockwise)
            {
                nextIndex = _wheel.MoveIndexCounterClockwise(index);
            }
            if (_wheel.IsPointEmpty(index))
            {
                continue;
            }
            RotationMove rotationMove = new RotationMove();
            rotationMove.AssignType(type);
            rotationMove.AssignFromIndex(index);
            rotationMove.AssignToIndex(nextIndex);

            rotationMoves.Add(rotationMove);
        }

        _wheel.MakeRotationMoves(rotationMoves, _moveLerpSpeed);
    }

    private void OnCurrentMoveCompleted()
    {
        _segmentToMove = null;
        WheelDelegates.ActionCheckWheelCompletion();
    }

    public override WheelState HandleTransitions()
    {
        if (_segmentToMove == null)
        {
            return WheelDelegates.IdleState();
        }
        else
        { 
            return null;
        }
    }

    public override void ProcessState()
    {
    }

    public override void OnDestroy()
    {
        WheelDelegates.MoveState -= GetThisState;
    }

    protected override WheelState GetThisState()
    {
        return this;
    }

    public override string ToString()
    {
        return "MoveState";
    }
}

/*
Debug.DrawRay(worldPos, DirToCenter * 10, Color.red, 10);
Debug.DrawRay(worldPos, worldDir * 10, Color.blue, 10);
Debug.DrawRay(worldPos, worldDir * 10, Color.green, 10);
Debug.Log(swipeAngle * Mathf.Rad2Deg);
*/