using Unity.Mathematics;

public class WheelRotationMove : FigureSegmentMove
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

    public override string ToString()
    {
        return $"Rotation move {Type} " + base.ToString();
    }
}