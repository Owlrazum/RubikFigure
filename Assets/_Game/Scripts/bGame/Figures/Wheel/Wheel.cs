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

    public void AssignTransitionDatas(Array2D<WheelSegmentTransitions> transitions)
    {
        _transitions = transitions;
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
        int sideDelta = to.x - from.x;
        int ringDelta = to.y - from.y;
        if (sideDelta > 0)
        {
            verticesMove.Transition = _transitions.GetElementByRef(to).AntiCW;
            verticesMove.ShouldReorientVertices = true;
        }

        if (sideDelta < 0)
        { 
            verticesMove.Transition = _transitions.GetElementByRef(to).CW;
        }

        if (ringDelta > 0)
        { 
            verticesMove.Transition = _transitions.GetElementByRef(to).Up;
        }

        if (ringDelta < 0)
        { 
            verticesMove.Transition = _transitions.GetElementByRef(to).Down;
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
