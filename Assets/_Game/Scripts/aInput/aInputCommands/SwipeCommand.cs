using UnityEngine;

public class SwipeCommand : InputCommand
{ 
    public Vector2 ViewEndPos { get; private set; }
    public Vector2 ViewStartPos { get; private set; }

    public void SetViewEndPos(Vector2 viewEndPosArg)
    {
        ViewEndPos = viewEndPosArg;
    }

    public void SetViewStartPos(Vector2 viewStartPosArg)
    {
        ViewStartPos = viewStartPosArg;
    }
}