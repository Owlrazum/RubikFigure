public class WheelVerticesMove : FigureSegmentMove
{
    public enum TypeType
    { 
        Up,
        Down
    }

    public TypeType Type { get; private set; }
    public void AssignType(TypeType type)
    {
        Type = type;
    }

    public WheelSegmentMesh VertexPositions { get; private set; }
    public void AssignVertexPositions(WheelSegmentMesh segmentVertexPositions)
    {
        VertexPositions = segmentVertexPositions;
    }
}
