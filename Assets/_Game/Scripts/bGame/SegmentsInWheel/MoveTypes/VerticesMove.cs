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

    public SegmentMesh VertexPositions { get; private set; }
    public void AssignVertexPositions(SegmentMesh segmentVertexPositions)
    {
        VertexPositions = segmentVertexPositions;
    }
}
