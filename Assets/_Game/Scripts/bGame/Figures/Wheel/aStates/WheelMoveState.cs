using System.Collections.Generic;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Math;

using static Orazum.Constants.Math;

public class WheelMoveState : FigureMoveState
{
    private Wheel _wheel;

    public WheelMoveState(WheelStatesController statesController, Wheel wheel, float moveLerpSpeed)
        : base(statesController, wheel, moveLerpSpeed)
    {
        _wheel = wheel;
        _movesToMake = new List<FigureSegmentMove>(wheel.SideCount);
    }

    protected override List<FigureSegmentMove> DetermineMovesFromInput(Vector3 worldPos, Vector3 worldDir)
    {
        _movesToMake.Clear();

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
            return ConstructVerticesMove(_currentSelectedPoint.Index, ClockOrderType.CW);
        }
        else if (swipeAngle > -5 * TAU / 12 && swipeAngle < -TAU / 12)
        {
            return ConstructVerticesMove(_currentSelectedPoint.Index, ClockOrderType.AntiCW);
        }
        else
        {
            return ConstructVerticesMove(_currentSelectedPoint.Index, VertOrderType.Up);
        }
    }

    private List<FigureSegmentMove> ConstructVerticesMove(int2 index, VertOrderType vertOrder)
    {
        int2 originIndex = index;
        int2 targetIndex = _wheel.MoveIndexVertOrder(originIndex, vertOrder);
        for (int side = 0; side < _wheel.SideCount; side++)
        {
            FigureVerticesMove verticesMove = new FigureVerticesMove();
            verticesMove.AssignFromIndex(originIndex);
            verticesMove.AssignToIndex(targetIndex);
            verticesMove.AssignLerpSpeed(_moveLerpSpeed);
            _movesToMake.Add(verticesMove);

            originIndex = targetIndex;
            targetIndex = _wheel.MoveIndexVertOrder(targetIndex, vertOrder);
        }

        PrintMovesToMakeIndices();
        return _movesToMake;
    }

    private List<FigureSegmentMove> ConstructVerticesMove(int2 index, ClockOrderType clockOrder)
    {
        int2 originIndex = index;
        int2 targetIndex = _wheel.MoveIndexInClockOrder(originIndex, clockOrder);
        for (int side = 0; side < _wheel.SideCount; side++)
        {
            FigureVerticesMove verticesMove = new FigureVerticesMove();
            verticesMove.AssignFromIndex(originIndex);
            verticesMove.AssignToIndex(targetIndex);
            verticesMove.AssignLerpSpeed(_moveLerpSpeed);
            _movesToMake.Add(verticesMove);

            originIndex = targetIndex;
            targetIndex = _wheel.MoveIndexInClockOrder(targetIndex, clockOrder);
        }

        return _movesToMake;
    }

    private void MoveIndexInRotationOrder(ref int2 index, ClockOrderType clockOrder)
    {
        index = _wheel.MoveIndexInClockOrder(index, clockOrder);
    }
}