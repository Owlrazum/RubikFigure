using System.Collections.Generic;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Math;

using static Orazum.Math.MathUtilities;

public class WheelMoveState : FigureMoveState
{
    private Wheel _wheel;

    public WheelMoveState( WheelStatesController statesController, Wheel wheel, float moveLerpSpeed)
        : base (statesController, wheel, moveLerpSpeed)
    {
        _wheel = wheel;
        _movesToMake = new List<FigureSegmentMove>(wheel.SideCount);
    }

     protected override List<FigureSegmentMove> DetermineMovesFromInput(Vector3 worldPos, Vector3 worldDir)
    {
        _movesToMake.Clear();
        Debug.Log("Determining");

        Vector3 DirToCenter = (_figureCenter - worldPos).normalized;
        float rotateAngleDeg = Mathf.Atan2(DirToCenter.z, DirToCenter.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(rotateAngleDeg, Vector3.up);
        worldDir = rotation * worldDir;
        float swipeAngle = Mathf.Atan2(worldDir.z, worldDir.x);
        if (swipeAngle > -TAU / 12 && swipeAngle < TAU / 12)
        {
            return ConstructVerticesMove(_currentSelectedPoint.Index, VertOrder.Down);
        }
        else if (swipeAngle > TAU / 12 && swipeAngle < 5 * TAU / 12)
        {
            return ConstructRotationMoves(_currentSelectedPoint.Index.y, ClockOrderType.CW);
        }
        else if (swipeAngle > -5 * TAU / 12 && swipeAngle < -TAU / 12)
        {
            return ConstructRotationMoves(_currentSelectedPoint.Index.y, ClockOrderType.CCW);
        }
        else
        {
            return ConstructVerticesMove(_currentSelectedPoint.Index, VertOrder.Up);
        }
    }

    private List<FigureSegmentMove> ConstructVerticesMove(int2 index, VertOrder vertOrder)
    {
        if (_wheel.IsValidIndexVertOrder(index, vertOrder))
        {
            WheelVerticesMove verticesMove = new WheelVerticesMove();
            verticesMove.AssignFromIndex(index);
            verticesMove.AssignToIndex(_wheel.MoveIndexVertOrder(index, vertOrder));
            verticesMove.AssignLerpSpeed(_moveLerpSpeed);
            _movesToMake.Add(verticesMove);
            return _movesToMake;
        }
        else
        {
            return null;
        }
    }

    private List<FigureSegmentMove> ConstructRotationMoves(int ringIndex, ClockOrderType clockOrder)
    {
        int2 index = new int2(0, ringIndex);
        int2 nextIndex = new int2(0, ringIndex);
        MoveIndexInRotationOrder(ref nextIndex, clockOrder);

        for (int i = 0; i < _wheel.SideCount; i++)
        {
            index = nextIndex;
            MoveIndexInRotationOrder(ref nextIndex, clockOrder);
            if (_wheel.IsPointEmpty(index))
            {
                continue;
            }
            WheelRotationMove rotationMove = new WheelRotationMove();
            rotationMove.AssignFromIndex(index);
            rotationMove.AssignToIndex(nextIndex);
            rotationMove.AssignLerpSpeed(_moveLerpSpeed);
            _movesToMake.Add(rotationMove);
        }

        return _movesToMake;
    }

    private void MoveIndexInRotationOrder(ref int2 index, ClockOrderType clockOrder)
    {
        index = _wheel.MoveIndexInClockOrder(index, clockOrder);
    }
}

/*
Debug.DrawRay(worldPos, DirToCenter * 10, Color.red, 10);
Debug.DrawRay(worldPos, worldDir * 10, Color.blue, 10);
Debug.DrawRay(worldPos, worldDir * 10, Color.green, 10);
Debug.Log(swipeAngle * Mathf.Rad2Deg);
*/