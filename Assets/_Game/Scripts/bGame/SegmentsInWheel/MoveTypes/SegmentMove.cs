using Unity.Mathematics;

public abstract class SegmentMove
{
    public int2 FromIndex { get; private set; }
    public int2 ToIndex { get; private set; }

    public void AssignFromIndex(int2 fromIndex)
    {
        FromIndex = fromIndex;
    }

    public void AssignToIndex(int2 toIndex)
    {
        ToIndex = toIndex;
    }

    public SegmentMove()
    {
        FromIndex = -1;
        ToIndex = -1;
    }

    public override string ToString()
    {
        return FromIndex + " " + ToIndex;
    }
}