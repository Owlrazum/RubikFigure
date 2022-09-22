using UnityEngine;
using Orazum.Meshing;

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