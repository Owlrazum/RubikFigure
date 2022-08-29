using Unity.Mathematics;

public class SwipeCommand : InputCommand
{ 
    public float2 ViewStartPos { get; private set; }
    public float2 ViewEndPos { get; private set; }

    public SwipeCommand(float2 startViewPos, float2 endViewPos)
    {
        ViewStartPos = startViewPos;
        ViewEndPos = endViewPos;
    }

    public SwipeCommand(float4 startEndViewPos)
    {
        ViewStartPos = startEndViewPos.xy;
        ViewEndPos = startEndViewPos.zw;
    }
}