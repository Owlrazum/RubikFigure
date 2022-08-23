public class WheelVerticesMove : FigureSegmentMove
{
    public WheelSegmentMesh SegmentMesh { get; private set; }
    public void AssignSegmentMesh(WheelSegmentMesh segmentVertexPositions)
    {
        SegmentMesh = segmentVertexPositions;
    }
}
