using System;
using Unity.Mathematics;

public class SegmentMove
{
    public SegmentMoveType MoveType { get; private set; }
    public int2 FromIndex { get; private set; }
    public int2 ToIndex { get; private set; }

    public SegmentMove(SegmentMoveType typeArg, int2 fromIndexArg, int2 toIndexArg)
    {
        MoveType = typeArg;
        FromIndex = fromIndexArg;
        ToIndex = toIndexArg;

        IsValid = false;
        EmtpyPointIndex = -1;
    }

    public bool IsValid { get; set; }
    public int EmtpyPointIndex { get; set; }

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
        if (!IsValid)
        {
            return "Not valid";
        }

        return MoveType + " " + FromIndex + " " + ToIndex + " " + EmtpyPointIndex;
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