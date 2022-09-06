using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WheelSegmentMover : FigureSegmentMover
{
    protected override int MaxVertexCount => 2 * (MeshResolution + 1);
    protected override int MaxIndexCount => 6 * MeshResolution;
}

public struct WheelSegmentTransitions
{ 
    private QS_Transition up;
    private QS_Transition down;
    private QS_Transition cw;
    private QS_Transition antiCW;

    public static ref QS_Transition Up(ref WheelSegmentTransitions instance)
    {
        return ref instance.up;
    }
    public static ref QS_Transition Down(ref WheelSegmentTransitions instance)
    {
        return ref instance.down;
    }
    public static ref QS_Transition CW(ref WheelSegmentTransitions instance)
    {
        return ref instance.cw;
    }
    public static ref QS_Transition AntiCW(ref WheelSegmentTransitions instance)
    {
        return ref instance.antiCW;
    }
}