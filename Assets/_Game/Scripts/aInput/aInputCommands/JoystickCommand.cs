using Unity.Mathematics;
public class JoystickCommand : InputCommand
{
    public float2 Joy { get; private set; }

    public JoystickCommand(float2 joy)
    {
        Joy = joy;
    }
}