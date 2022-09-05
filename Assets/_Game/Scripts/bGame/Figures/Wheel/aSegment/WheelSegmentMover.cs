using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WheelSegmentMover : FigureSegmentMover
{
    protected override int MaxVertexCount => 2 * (MeshResolution + 1);
    protected override int MaxIndexCount => 6 * MeshResolution;
}

public struct WheelSegmentTransitions
{ 
    private QS_Transition atsi;
    private QS_Transition ctsi;
    private QS_Transition dtsi;
    private QS_Transition utsi;

    public static ref QS_Transition Atsi(ref WheelSegmentTransitions instance)
    {
        return ref instance.atsi;
    }
    public static ref QS_Transition Ctsi(ref WheelSegmentTransitions instance)
    {
        return ref instance.ctsi;
    }
    public static ref QS_Transition Dtsi(ref WheelSegmentTransitions instance)
    {
        return ref instance.dtsi;
    }
    public static ref QS_Transition Utsi(ref WheelSegmentTransitions instance)
    {
        return ref instance.utsi;
    }
}