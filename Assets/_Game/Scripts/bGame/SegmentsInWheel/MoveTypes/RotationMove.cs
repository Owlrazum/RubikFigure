using Unity.Mathematics;

public class RotationMove : SegmentMove
{
    public enum TypeType
    { 
        Clockwise,
        CounterClockwise
    }
    
    public TypeType Type { get; private set; }
    public void AssignType(TypeType type)
    {
        Type = type;
    }

    public quaternion Rotation { get; private set; }
    public void AssignRotation(quaternion rotation)
    {
        Rotation = rotation;
    }
}