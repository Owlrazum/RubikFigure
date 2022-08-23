using System;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;
using static Orazum.Math.MathUtilities;

public class Valknut : Figure
{
    public const int TrianglesCount = 3;
    public const int TriangleSegmentsCount = 2;

    private ValknutSegmentMesh[] _segmentMeshes;

    public void Initialize(
        Array2D<FigureSegmentPoint> segmentPoints, 
        ValknutSegmentMesh[] segmentMeshes, 
        ValknutStatesController statesController,
        FigureParamsSO figureParams
    )
    { 
        _segmentPoints = segmentPoints;
        _segmentMeshes = segmentMeshes;

        _startTeleportPosition = figureParams.StartPositionForSegmentsInCompletionPhase;

        statesController.Initialize(this, figureParams);
    }

    protected override void MakeSegmentMove(FigureSegment segment, FigureSegmentMove move, Action moveCompleteAction)
    {
        throw new NotImplementedException();
    }

    protected override string GetFigureName()
    {
        return "Valknut";
    }
}