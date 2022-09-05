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
    private QS_Transition CW;
    private QS_Transition antiCW;

    public static ref QS_Transition Clockwise (ref ValknutSegmentTransitions instance)
    {
        return ref instance.CW;
    }
    public static ref QS_Transition AntiClockwise (ref ValknutSegmentTransitions instance)    
    {
        return ref instance.antiCW;
    }
}