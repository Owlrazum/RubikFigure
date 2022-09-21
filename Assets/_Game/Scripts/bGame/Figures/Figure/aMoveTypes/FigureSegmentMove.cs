using Unity.Mathematics;

public class FigureSegmentMove
{
    public int2 FromIndex { get; protected set; }
    public int2 ToIndex { get; protected set; }
    public float LerpSpeed { get; protected set; }

    public void AssignFromIndex(int2 fromIndex)
    {
        FromIndex = fromIndex;
    }

    public void AssignToIndex(int2 toIndex)
    {
        ToIndex = toIndex;
    }

    public void AssignLerpSpeed(float lerpSpeed)
    {
        LerpSpeed = lerpSpeed;
    }

    public FigureSegmentMove()
    {
        FromIndex = -1;
        ToIndex = -1;
    }

    public override string ToString()
    {
        return FromIndex + " " + ToIndex;
    }
}