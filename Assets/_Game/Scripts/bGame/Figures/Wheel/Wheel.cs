using System;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Meshing;
using Orazum.Collections;
using static Orazum.Constants.Math;

public class Wheel : Figure
{
    public int SideCount { get { return _dims.x; } }
    public int RingCount { get { return _dims.y; } }

    private Array2D<WheelSegmentTransitions> _transitions;

    public void AssignTransitionDatas(in Array2D<WheelSegmentTransitions> transitions)
    {
        _transitions = transitions;
        for (int i = 0; i < transitions.RowCount; i++)
        {
            for (int j = 0; j < transitions.ColCount; j++)
            {
                bool b1 = transitions[j, i].CW.IsCreated && transitions[j, i].AntiCW.IsCreated;
                bool b2 = transitions[j, i].Up.IsCreated && transitions[j, i].Down.IsCreated;
                if (!b1 || !b2)
                {
                    Debug.LogWarning($"{j} {i} not created transition");
                }
                // Assert.IsTrue(b1, $"{j} {i} not created transition");
                // Assert.IsTrue(b2, $"{j} {i} not created transition");
            }
        }
    }

    public override void Initialize(
        Array2D<FigureSegmentPoint> segmentPoints,
        FigureParamsSO figureParams)
    {
        base.Initialize(segmentPoints, figureParams);
        RotateSegmentPointsOnGeneration();
    }
    private void RotateSegmentPointsOnGeneration()
    {
        for (int side = 0; side < _dims.x; side++)
        {
            for (int ring = 0; ring < _dims.y; ring++)
            {
                Quaternion rotation = GetSideRotation(side);
                int2 index = new int2(side, ring);
                _segmentPoints[index].transform.localRotation = rotation;
            }
        }
    }

    private Quaternion GetSideRotation(int2 index)
    {
        return GetSideRotation(index.x);
    }
    private Quaternion GetSideRotation(int sideIndex)
    {
        float rotationAngle = sideIndex * TAU / SideCount * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(rotationAngle, Vector3.up);
    }

    protected override void MakeSegmentMove(FigureSegment segment, FigureSegmentMove move, Action moveCompleteAction)
    {
        Assert.IsTrue(IsValidIndex(move.FromIndex) && IsValidIndex(move.ToIndex));
        if (move is FigureVerticesMove verticesMove)
        {
            AssignTransData(verticesMove);
        }
        // else if (move is WheelVerticesMove verticesMove)
        // { 
        //     Assert.IsNull(_segmentPoints[move.ToIndex].Segment);
        //     Assert.IsNotNull(_segmentPoints[move.FromIndex].Segment);
        //     AssignSegmentMesh(verticesMove);
        // }
        // else if (move is WheelTeleportMove teleportMove)
        // {
        //     AssignTeleportMoveData(teleportMove);
        // }

        segment.StartMove(move, moveCompleteAction);
    }

    private void AssignTransData(FigureVerticesMove verticesMove)
    {
        int2 from = verticesMove.FromIndex;
        int2 to = verticesMove.ToIndex;
        Assert.IsTrue(from.x == to.x || from.y == to.y);
        int sideDelta = to.x - from.x;
        int ringDelta = to.y - from.y;

        if (sideDelta > 0)
        {
            if (sideDelta == _dims.x - 1)
            { 
                verticesMove.Transition = _transitions[to].AntiCW;    
            }
            else
            { 
                verticesMove.Transition = _transitions[to].CW;
            }
        }

        if (sideDelta < 0)
        {
            if (sideDelta == -(_dims.x - 1))
            {
                verticesMove.Transition = _transitions[to].CW;
            }
            else
            {
                verticesMove.Transition = _transitions[to].AntiCW;
            }
        }

        if (ringDelta > 0)
        {
            if (ringDelta == _dims.y - 1)
            { 
                verticesMove.Transition = _transitions[to].Down;
            }
            else
            { 
                verticesMove.Transition = _transitions[to].Up;
            }
        }

        if (ringDelta < 0)
        {
            if (ringDelta == -(_dims.y - 1))
            {
                verticesMove.Transition = _transitions[to].Up;
            }
            else
            { 
                verticesMove.Transition = _transitions[to].Down;
            }
        }
    }

    [ContextMenu("Print")]
    public void Print()
    {
        Debug.Log(ToString());
    }

    protected override string GetFigureName()
    {
        return "Wheel";
    }
}
