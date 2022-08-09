using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;
using Orazum.Utilities;
using static Orazum.Utilities.MathUtilities;

public class MoveState : WheelState
{
    private SwipeCommand _currentSwipeCommand;
    private float _moveLerpSpeed;

    public MoveState(WheelGenerationData generationData) : base(generationData)
    { 
        _moveLerpSpeed = generationData.LevelDescriptionSO.MoveLerpSpeed;

        WheelStatesDelegates.MoveState += GetThisState;
    }

    public void AssignSwipeCommand(SwipeCommand swipeCommand)
    {
        _currentSwipeCommand = swipeCommand;
    }

    public override void OnEnter(Wheel wheel)
    {
        Assert.IsNotNull(_currentSwipeCommand);

        Camera renderingCamera = GameDelegatesContainer.GetRenderingCamera();
        Vector3 center = wheel.transform.position;
        float planeDistance = (center - renderingCamera.transform.position).magnitude;
        Vector3 viewStartPos = new Vector3(_currentSwipeCommand.ViewStartPos.x, 
            _currentSwipeCommand.ViewStartPos.y, planeDistance);
        Vector3 viewEndPos = new Vector3(_currentSwipeCommand.ViewEndPos.x, 
            _currentSwipeCommand.ViewEndPos.y, planeDistance);

        Ray ray = renderingCamera.ViewportPointToRay(viewStartPos);
        if (Physics.Raycast(ray, out RaycastHit hitInfo, 1000, LayerUtilities.SEGMENT_POINTS_LAYER_MASK, QueryTriggerInteraction.Collide))
        { 

        }

        Vector3 worldStartPos = renderingCamera.ViewportToWorldPoint(viewStartPos);
        Vector3 worldEndPos = renderingCamera.ViewportToWorldPoint(viewEndPos);
        Vector3 worldDir = (worldEndPos - worldStartPos).normalized;

        int2[] emptyIndices = wheel.GetEmptyIndices();

        int minDistanceIndex = -1;
        float minDistance = -1;
        for (int i = 0; i < emptyIndices.Length; i++)
        {
            Vector3 emptyPoint = wheel.GetEmptySegmentPointPosition(i);
            float distance = (emptyPoint - worldStartPos).sqrMagnitude;
            if (minDistance < 0)
            {
                minDistance = distance;
                minDistanceIndex = i;
            }
            else if (minDistance > distance)
            {
                minDistance = distance;
                minDistanceIndex = i;
            }
        }

        int2 closestEmptyPointIndex = emptyIndices[minDistanceIndex];
        SegmentMoveType moveType = DetermineMoveType(center, worldStartPos, worldDir);
        Debug.Log(moveType + " " + closestEmptyPointIndex);
        _currentSwipeCommand = null;
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

    public override WheelState HandleTransitions()
    {
        if (_currentSwipeCommand == null)
        {
            return WheelStatesDelegates.IdleState();
        }
        else
        { 
            return null;
        }
    }

    public override void StartProcessingState(Wheel wheel)
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