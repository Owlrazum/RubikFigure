using Unity.Mathematics;

public class ValknutSegment : FigureSegment
{
    public float3 EndPointCW { get; private set; }
    public float3 EndPointAntiCW { get; private set; }
    public void AssignEndPoints(float3 cw, float3 antiCw)
    {
        EndPointCW = cw;
        EndPointAntiCW = antiCw;
    }
}