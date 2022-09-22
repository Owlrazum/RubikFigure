using Orazum.Meshing;

/// <summary>
/// The clockOrder is determined by the origin segment mesh.
/// </summary>
public struct ValknutSegmentTransitions
{
    public QS_Transition CW;
    public QS_Transition AntiCW;

    public override string ToString()
    {
        return $"ValknutSegmentTransitions:\n" +
            $"Clockwise: {CW}; AntiClockwise: {AntiCW}"
        ;
    }
}