using Unity.Mathematics;
using UnityEngine;

public class ValknutSegment : FigureSegment
{
    protected override void InitializeMover()
    {
        _mover = gameObject.AddComponent<ValknutSegmentMover>();
    }

    public float3 EndPointCW { get; private set; }
    public float3 EndPointAntiCW { get; private set; }
    public void AssignEndPoints(float3 cw, float3 antiCw)
    {
        EndPointCW = cw;
        EndPointAntiCW = antiCw;
    }
}