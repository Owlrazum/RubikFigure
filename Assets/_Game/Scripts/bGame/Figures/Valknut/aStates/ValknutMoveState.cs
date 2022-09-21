using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Orazum.Math;
using static Orazum.Collections.IndexUtilities;
using static Orazum.Math.LineSegmentUtilities;

public class ValknutMoveState : FigureMoveState
{
    public ValknutMoveState(ValknutStatesController statesController, Valknut valknut, float moveLerpSpeed) :
        base(statesController, valknut, moveLerpSpeed)
    {
        _movesToMake = new List<FigureSegmentMove>(1);
    }

    protected override List<FigureSegmentMove> DetermineMovesFromInput(Vector3 worldPos, Vector3 worldDir)
    {
        _movesToMake.Clear();
        ValknutSegment v = _currentSelectedPoint.Segment as ValknutSegment;
        float cwDist = DistanceLineSegment(v.EndPointCW, worldPos);
        float antiCwDist = DistanceLineSegment(v.EndPointAntiCW, worldPos);
        if (cwDist >= antiCwDist)
        {
            return ConstructVerticesMove(_currentSelectedPoint.Index, ClockOrderType.CW);
        }
        else
        {
            return ConstructVerticesMove(_currentSelectedPoint.Index, ClockOrderType.AntiCW);
        }
    }

    private List<FigureSegmentMove> ConstructVerticesMove(int2 index, ClockOrderType clockOrder)
    {
        int2 originIndex = index;
        int2 targetIndex = index;
        IncreaseIndex(ref targetIndex.x, 3);
        if (clockOrder == ClockOrderType.AntiCW)
        {
            IncreaseIndex(ref targetIndex.y, 2);
        }

        FigureVerticesMove verticesMove = new FigureVerticesMove();
        verticesMove.AssignFromIndex(originIndex);
        verticesMove.AssignToIndex(targetIndex);
        verticesMove.AssignLerpSpeed(_moveLerpSpeed);
        _movesToMake.Add(verticesMove);

        return _movesToMake;
    }
}