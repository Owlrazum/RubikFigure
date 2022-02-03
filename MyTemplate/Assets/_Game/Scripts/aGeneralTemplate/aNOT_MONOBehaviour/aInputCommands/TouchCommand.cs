using UnityEngine;

public class TouchCommand : InputCommand
{
    public Vector3 TouchScreenPos { get; private set; }
    public TouchCommand(Vector3 screenPos)
    {
        TouchScreenPos = screenPos;
        RenderingCamera = null;
    }

    public Camera  RenderingCamera { get; set; }
}
