using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WheelSegmentMover : FigureSegmentMover
{
    protected override int MaxVertexCount => 2 * (MeshResolution + 1);
    protected override int MaxIndexCount => 6 * MeshResolution;
}

public struct WheelSegmentTransitions
{ 
    private QSTransition atsi;
    private QSTransition ctsi;
    private QSTransition dtsi;
    private QSTransition utsi;

    public static ref QSTransition Atsi(ref WheelSegmentTransitions instance)
    {
        return ref instance.atsi;
    }
    public static ref QSTransition Ctsi(ref WheelSegmentTransitions instance)
    {
        return ref instance.ctsi;
    }
    public static ref QSTransition Dtsi(ref WheelSegmentTransitions instance)
    {
        return ref instance.dtsi;
    }
    public static ref QSTransition Utsi(ref WheelSegmentTransitions instance)
    {
        return ref instance.utsi;
    }
}