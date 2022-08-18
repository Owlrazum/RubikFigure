public class ValknutVerticesMove : FigureSegmentMove
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

    public ValknutSegmentMesh VertexPositions { get; private set; }
    public void AssignVertexPositions(ValknutSegmentMesh segmentVertexPositions)
    {
        VertexPositions = segmentVertexPositions;
    }
}
