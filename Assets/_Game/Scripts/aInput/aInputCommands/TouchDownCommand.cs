using Unity.Mathematics;

public class TouchDownCommand : InputCommand
{
    public float2 TouchViewPos { get; private set; }
    public TouchDownCommand(float2 viewPos)
    {
        TouchViewPos = viewPos;
    }
}
