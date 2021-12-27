using UnityEngine;

public class TouchCommand
{
    private Vector2 screenPos;
    private Ray screenToWorldRay;

    public TouchCommand(Vector2 screenPosArg)
    {
        screenPos = screenPosArg;
    }

    public TouchCommand(int screenX, int screenY)
        : this(new Vector2(screenX, screenY))
    {
    }

    public Vector2 GetScreenPos()
    {
        return screenPos;
    }

    public void SetScreenToWorldRay(Ray ray)
    {
        screenToWorldRay = ray;
    }

    public Ray GetScreenToWorldRay()
    {
        return screenToWorldRay;
    }
}
