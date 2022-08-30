public class WheelVerticesMove : FigureSegmentMove
{
    public enum MoveMethod
    {
        Grounded,
        LevitationDown,
        LevitationUp
    }
    public MoveMethod Method { get; private set; }
    public void AssignMethod(MoveMethod method)
    {
        Method = method;
    }

public WheelSegmentMesh SegmentMesh { get; private set; }
    public void AssignSegmentMesh(WheelSegmentMesh segmentVertexPositions)
    {
        SegmentMesh = segmentVertexPositions;
    }
}
