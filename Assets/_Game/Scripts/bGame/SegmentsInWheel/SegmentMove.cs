using System;
using Unity.Mathematics;
using UnityEngine.Assertions;

public enum SegmentMoveType
{ 
    Down,
    Up,
    CounterClockwise,
    Clockwise
}

public class SegmentMove
{
    public SegmentMoveType Type { get; set; }
    public int2 FromIndex { get; set; }
    public int2 ToIndex { get; set; }
    private SegmentPoint _target;

    public SegmentMove(SegmentMoveType typeArg, int2 fromIndexArg, int2 toIndexArg)
    {
        Type = typeArg;
        FromIndex = fromIndexArg;
        ToIndex = toIndexArg;
    }

    public SegmentVertexPositions VertexPositions { get; private set; }
    public void AssignVertexPositions(SegmentVertexPositions vertexPositionsArg)
    {
        Assert.IsTrue(Type == SegmentMoveType.Down || Type == SegmentMoveType.Up);
        VertexPositions = vertexPositionsArg;
    }

    public override string ToString()
    {
        return Type + " " + FromIndex + " " + ToIndex;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is SegmentMove move))
        {
            return false;
        }

        if (math.all(this.FromIndex == move.FromIndex)
            && math.all(this.ToIndex == move.ToIndex)
            && this.Type == move.Type)
        {
            return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FromIndex, ToIndex, Type);
    }
}