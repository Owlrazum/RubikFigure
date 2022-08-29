using System;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;
using static Orazum.Math.MathUtilities;

public class Wheel : Figure
{
    private WheelSegmentMesh[] _segmentMeshes;
    public int SideCount { get { return _dims.x; } }
    public int RingCount { get { return _dims.y; } }

    public void AssignSegmentMeshes(WheelSegmentMesh[] segmentMeshes)
    {
        _segmentMeshes = segmentMeshes;
    }
    public override void Initialize(
        Array2D<FigureSegmentPoint> segmentPoints,
        FigureStatesController statesController,
        FigureParamsSO figureParams)
    {
        base.Initialize(segmentPoints, statesController, figureParams);
        RotateSegmentOnGeneration();
    }
    private void RotateSegmentOnGeneration()
    {
        for (int side = 0; side < _dims.x; side++)
        {
            for (int ring = 0; ring < _dims.y; ring++)
            {
                Quaternion rotation = GetSideRotation(side);
                int2 index = new int2(side, ring);
                _segmentPoints[index].transform.localRotation = rotation;
                if (_segmentPoints[index].Segment != null)
                { 
                    _segmentPoints[index].Segment.transform.localRotation = rotation;
                }
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
        if (move is WheelRotationMove rotationMove)
        { 
            AssignRotation(rotationMove);
        }
        else if (move is WheelVerticesMove verticesMove)
        { 
            Assert.IsNull(_segmentPoints[move.ToIndex].Segment);
            Assert.IsNotNull(_segmentPoints[move.FromIndex].Segment);
            AssignSegmentMesh(verticesMove);
        }
        else if (move is WheelTeleportMove teleportMove)
        {
            AssignTeleportMoveData(teleportMove);
        }

        segment.StartMove(move, moveCompleteAction);
    }

    private void AssignRotation(WheelRotationMove rotationMove)
    {
        quaternion from = GetSideRotation(rotationMove.FromIndex);
        quaternion to = GetSideRotation(rotationMove.ToIndex);
        quaternion diff = math.mul(math.inverse(from), to);
        rotationMove.AssignRotation(diff);
    }

    private void AssignSegmentMesh(WheelVerticesMove verticesMove)
    { 
        verticesMove.AssignSegmentMesh(_segmentMeshes[verticesMove.ToIndex.y]);
    }

    private void AssignTeleportMoveData(WheelTeleportMove teleportMove)
    { 
        teleportMove.AssignTargetOrientation(GetSideRotation(teleportMove.ToIndex));
        teleportMove.AssignStartTeleportPosition(_startTeleportPosition);
        teleportMove.AssignSegmentMesh(_segmentMeshes[teleportMove.ToIndex.y]);
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
