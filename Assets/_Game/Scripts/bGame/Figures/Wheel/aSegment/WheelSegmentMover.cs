using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WheelSegmentMover : FigureSegmentMover
{
    protected override int MaxVertexCount => 2 * (MeshResolution + 1);
    protected override int MaxIndexCount => 6 * MeshResolution;
}