using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using static Orazum.Math.MathUtilities;

public class MoveState : WheelState
{
    private SwipeCommand _currentSwipeCommand;
    private SegmentPoint _currentSelectedPoint;
    private Segment _segmentToMove;
    private SegmentMove _moveToMake; // we store it once to avoid gc. SegemntToMove presence determines logic.
    private float _moveLerpSpeed;

    public MoveState(LevelDescriptionSO levelDescription, Wheel wheelArg) : base(levelDescription, wheelArg)
    { 
        _moveLerpSpeed = levelDescription.MoveLerpSpeed;
        _moveToMake = new SegmentMove(SegmentMoveType.Down, int2.zero, int2.zero);

        WheelStatesDelegates.MoveState += GetThisState;
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

        SegmentMoveType moveType = DetermineMoveType(center, worldStartPos, worldDir);
        _moveToMake.FromIndex = _currentSelectedPoint.Index;
        _moveToMake.MoveType = moveType;
        if (_wheel.IsMovePossible(_moveToMake, out int2 toIndex))
        {
            _segmentToMove = _currentSelectedPoint.Segment;
            _moveToMake.ToIndex = toIndex;
            _wheel.MakeMove(in _moveToMake, _moveLerpSpeed, OnCurrentMoveCompleted);
        }
        else
        {
            _segmentToMove = null;
        }
    }

    private SegmentMoveType DetermineMoveType(Vector3 circleCenter, Vector3 worldPos, Vector3 worldDir)
    {
        Vector3 DirToCenter = (circleCenter - worldPos).normalized;
        float rotateAngleDeg = Mathf.Atan2(DirToCenter.z, DirToCenter.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(rotateAngleDeg, Vector3.up);
        worldDir = rotation * worldDir;
        float swipeAngle = Mathf.Atan2(worldDir.z, worldDir.x);

        if (swipeAngle > -TAU / 8 && swipeAngle < TAU / 8)
        { 
            return SegmentMoveType.Down;
        }
        else if (swipeAngle > TAU / 8 && swipeAngle < 3 * TAU / 8)
        {
            return SegmentMoveType.Clockwise;
        }
        else if (swipeAngle > -3 * TAU / 8 && swipeAngle < -TAU / 8)
        {
            return SegmentMoveType.CounterClockwise;
        }
        else
        {
            return SegmentMoveType.Up;
        }
    }

    private void OnCurrentMoveCompleted()
    {
        _segmentToMove = null;
    }

    public override WheelState HandleTransitions()
    {
        if (_segmentToMove == null)
        {
            return WheelStatesDelegates.IdleState();
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
        WheelStatesDelegates.MoveState -= GetThisState;
    }

    protected override WheelState GetThisState()
    {
        return this;
    }

    
}

/*
Debug.DrawRay(worldPos, DirToCenter * 10, Color.red, 10);
Debug.DrawRay(worldPos, worldDir * 10, Color.blue, 10);
Debug.DrawRay(worldPos, worldDir * 10, Color.green, 10);
Debug.Log(swipeAngle * Mathf.Rad2Deg);
*/