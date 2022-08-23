using Unity.Mathematics;

public class WheelTeleportMove : FigureSegmentMove
{ 
    public WheelSegmentMover SegmentMover { get; private set; }
    public void AssignSegmentMover(WheelSegmentMover segmentMover)
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

    public WheelSegmentMesh SegmentMesh { get; private set; }
    public void AssignSegmentMesh(WheelSegmentMesh segmentMesh)
    {
        SegmentMesh = segmentMesh;
    }
}