using System;
using Unity.Mathematics;

public class SegmentMove
{
    public SegmentMoveType MoveType { get; set; }
    public int2 FromIndex { get; set; }
    public int2 ToIndex { get; set; }

    public SegmentMove(SegmentMoveType typeArg, int2 fromIndexArg, int2 toIndexArg)
    {
        MoveType = typeArg;
        FromIndex = fromIndexArg;
        ToIndex = toIndexArg;
    }

    private SegmentPoint _target;
    public SegmentPointCornerPositions GetTargetCornerPositions()
    {
        return _target.CornerPositions;
    }

    public void AssignTarget(SegmentPoint targetArg)
    {
        _target = targetArg;
    }

    public override string ToString()
    {
        return MoveType + " " + FromIndex + " " + ToIndex;
    }

    public override bool Equals(object obj)
    {
        if (!(obj is SegmentMove move))
        {
            return false;
        }

        if (math.all(this.FromIndex == move.FromIndex)
            && math.all(this.ToIndex == move.ToIndex)
            && this.MoveType == move.MoveType)
        {
            return true;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FromIndex, ToIndex, MoveType);
    }
}