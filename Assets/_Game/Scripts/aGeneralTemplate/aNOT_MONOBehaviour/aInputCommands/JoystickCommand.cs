public class JoystickCommand : InputCommand
{
    public float Horiz { get; private set; }
    public float Vert  { get; private set; }

    public JoystickCommand(float horiz, float vert)
    {
        Horiz = horiz;
        Vert = vert;
    }
}