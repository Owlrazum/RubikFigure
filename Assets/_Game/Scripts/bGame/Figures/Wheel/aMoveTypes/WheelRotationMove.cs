using Unity.Mathematics;
using UnityEngine;

using Orazum.Math;

public class WheelRotationMove : FigureSegmentMove
{
    public WheelRotationMove()
    {
        FromIndex = new int2(-1, -1);
        ToIndex = new int2(-1, -1);
    }

    public WheelRotationMove(FigureSegmentMove move)
    { 
        FromIndex = move.FromIndex;
        ToIndex = move.ToIndex;
        LerpSpeed = move.LerpSpeed;
        Mover = move.Mover;
        Rotation = quaternion.identity;
    }

    public quaternion Rotation { get; private set; }
    public void AssignRotation(quaternion rotation)
    {
        Rotation = rotation;
    }

    public ClockOrder RotationOrder { get; set; }

    public override string ToString()
    {
        Quaternion q = Rotation;
        return $"Rotation move {quaternion.Euler(q.eulerAngles)} " + base.ToString();
    }
}