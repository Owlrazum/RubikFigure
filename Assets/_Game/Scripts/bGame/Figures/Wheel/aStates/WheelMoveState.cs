using System.Collections.Generic;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Math;

using static Orazum.Constants.Math;

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
            return ConstructVerticesMove(_currentSelectedPoint.Index, VertOrderType.Down);
        }
        else if (swipeAngle > TAU / 12 && swipeAngle < 5 * TAU / 12)
        {
            return ConstructVerticesMove(_currentSelectedPoint.Index.y, ClockOrderType.CW);
        }
        else if (swipeAngle > -5 * TAU / 12 && swipeAngle < -TAU / 12)
        {
            return ConstructVerticesMove(_currentSelectedPoint.Index.y, ClockOrderType.AntiCW);
        }
        else
        {
            return ConstructVerticesMove(_currentSelectedPoint.Index, VertOrderType.Up);
        }
    }

    private List<FigureSegmentMove> ConstructVerticesMove(int2 index, VertOrderType vertOrder)
    {
        if (_wheel.IsValidIndexVertOrder(index, vertOrder))
        {
            FigureVerticesMove verticesMove = new FigureVerticesMove();
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

    private List<FigureSegmentMove> ConstructVerticesMove(int2 index, ClockOrderType clockOrder)
    {
        if (_wheel.IsValidIndexClockOrder(index, clockOrder))
        {
            FigureVerticesMove verticesMove = new FigureVerticesMove();
            verticesMove.AssignFromIndex(index);
            verticesMove.AssignToIndex(_wheel.MoveIndexInClockOrder(index, clockOrder));
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