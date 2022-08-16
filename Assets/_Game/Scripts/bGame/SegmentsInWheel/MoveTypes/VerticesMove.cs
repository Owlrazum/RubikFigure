public class VerticesMove : SegmentMove
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

    public SegmentVertexPositions VertexPositions { get; private set; }
    public void AssignVertexPositions(SegmentVertexPositions segmentVertexPositions)
    {
        VertexPositions = segmentVertexPositions;
    }
}
