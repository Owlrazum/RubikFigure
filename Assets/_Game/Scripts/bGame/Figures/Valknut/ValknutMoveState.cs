using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

using Orazum.Math;
using static Orazum.Collections.IndexUtilities;
using static Orazum.Math.LineSegmentUtilities;

public class ValknutMoveState : FigureMoveState
{
    private Valknut _valknut;
    public ValknutMoveState(ValknutStatesController statesController, Valknut valknut, float moveLerpSpeed) :
        base(statesController, valknut, moveLerpSpeed)
    {
        _valknut = valknut;
        _movesToMake = new List<FM_Segment>(1);
    }

    protected override List<FM_Segment> DetermineMovesFromInput(Vector3 worldPos, Vector3 worldDir)
    {
        _movesToMake.Clear();
        ValknutSegmentPoint valknutPoint = _currentSelectedPoint as ValknutSegmentPoint;
        float startDist = DistanceLineSegment(valknutPoint.Start, worldPos);
        float endDist = DistanceLineSegment(valknutPoint.End, worldPos);
        DrawLineSegmentWithRaysUp(valknutPoint.Start, worldPos, 1, 10);

        if (startDist <= endDist)
        {
            return ConstructVerticesMove(_currentSelectedPoint.Index, LineEndType.Start);
        }
        else
        {
            return ConstructVerticesMove(_currentSelectedPoint.Index, LineEndType.End);
        }
    }

    private List<FM_Segment> ConstructVerticesMove(int2 index, LineEndType segmentEndPoint)
    {
        int2 originIndex = index;
        int2 targetIndex = index;
        IncreaseIndex(ref targetIndex.x, 3);
        if (segmentEndPoint == LineEndType.Start)
        { 
            IncreaseIndex(ref targetIndex.y, 2);
        }

        if (!_valknut.IsPointEmpty(targetIndex))
        {
            Debug.Log($"{targetIndex} is not empty\n{index} {segmentEndPoint}");
            return null;
        }

        FMSC_Transition verticesMove = new FMSC_Transition();
        verticesMove.AssignFromIndex(originIndex);
        verticesMove.AssignToIndex(targetIndex);
        verticesMove.AssignLerpSpeed(_moveLerpSpeed);
        _movesToMake.Add(verticesMove);

        return _movesToMake;
    }
}