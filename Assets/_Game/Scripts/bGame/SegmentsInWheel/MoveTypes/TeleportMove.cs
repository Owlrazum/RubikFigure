using Unity.Mathematics;

public class TeleportMove : SegmentMove
{ 
    public SegmentMover SegmentMover { get; private set; }
    public void AssignSegmentMover(SegmentMover segmentMover)
    {
        SegmentMover = segmentMover;
    }

    public quaternion TargetOrientation { get; private set; }
    public void AssignTargetOrientation(quaternion orientation)
    {
        TargetOrientation = orientation;
    }

    public float3 StartTeleportPosition { get; private set; }
    public void AssignStartTeleportPosition(float3 teleportPosition)
    {
        StartTeleportPosition = teleportPosition;
    }

    public SegmentVertexPositions VertexPositions { get; private set; }
    public void AssignVertexPositions(SegmentVertexPositions vertexPositions)
    {
        VertexPositions = vertexPositions;
    }
}