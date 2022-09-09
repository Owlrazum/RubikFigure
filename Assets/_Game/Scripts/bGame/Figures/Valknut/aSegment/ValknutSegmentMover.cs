using System;
using System.Collections;

using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Meshing;

[RequireComponent(typeof(MeshFilter))]
public class ValknutSegmentMover : FigureSegmentMover
{ 
    private const int MaxRangesCountForOneSegment = 7;
    protected override int MaxVertexCount { get { return (MaxRangesCountForOneSegment + 2) * 2; } }
    protected override int MaxIndexCount { get { return MaxRangesCountForOneSegment * 6; } }
}

/// <summary>
/// The clockOrder is determined by the origin segment mesh.
/// </summary>
public struct ValknutSegmentTransitions
{
    public QS_Transition CW;
    public QS_Transition AntiCW;

    public override string ToString()
    {
        return $"ValknutSegmentTransitions:\n" +
            $"Clockwise: {CW}; AntiClockwise: {AntiCW}"
        ;
    }
}