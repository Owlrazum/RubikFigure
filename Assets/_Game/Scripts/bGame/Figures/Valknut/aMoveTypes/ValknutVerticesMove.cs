using Orazum.Math;

public class ValknutVerticesMove : FigureSegmentMove
{
    public ClockOrderType ClockOrder { get; private set; }
    public void AssignClockOrder(ClockOrderType clockOrder)
    {
        ClockOrder = clockOrder;
    }

    public ValknutSegmentMesh VertexPositions { get; private set; }
    public void AssignVertexPositions(ValknutSegmentMesh segmentVertexPositions)
    {
        VertexPositions = segmentVertexPositions;
    }
}
