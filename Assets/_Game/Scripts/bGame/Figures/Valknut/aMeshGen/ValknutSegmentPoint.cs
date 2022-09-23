using Unity.Mathematics;

public class ValknutSegmentPoint : FigureSegmentPoint
{ 
    public float3 Start { get; private set; }
    public float3 End { get; private set; }
    public void AssignEndPoints(in float3 start, in float3 end)
    {
        Start = start;
        End = end;
    }
}