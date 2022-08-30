using System;
using System.Collections;

using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshFilter))]
public class ValknutSegmentMover : FigureSegmentMover
{ 
    protected override int MaxVertexCount { get { return (ValknutGenerator.MaxRangesCountForOneSegment + 2) * 2; } }
    protected override int MaxIndexCount { get { return ValknutGenerator.MaxRangesCountForOneSegment * 6; } }
}