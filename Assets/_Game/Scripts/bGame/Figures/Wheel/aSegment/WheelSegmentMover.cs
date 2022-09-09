using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class WheelSegmentMover : FigureSegmentMover
{
    protected override int MaxVertexCount => 2 * (MeshResolution + 1) * 2;
    protected override int MaxIndexCount => 6 * MeshResolution * 2;
}

public struct WheelSegmentTransitions
{
    public QS_Transition Up;
    public QS_Transition Down;
    public QS_Transition CW;
    public QS_Transition AntiCW;

    public override string ToString()
    {
        return $"WheelSegmentTransitions:\n" + 
            $"Up: {Up.Length}; Down: {Down.Length}\n" +
            $"Clockwise: {CW.Length}; AntiClockwise: {AntiCW.Length}"
        ;
    }
}