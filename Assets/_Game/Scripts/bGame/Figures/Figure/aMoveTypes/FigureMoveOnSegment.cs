using Unity.Mathematics;

public abstract class FigureMoveOnSegment
{ 
    public int2 Index { get; protected set; }
    public void AssignIndex(int2 segmentIndex)
    {
        Index = segmentIndex;
    }

    public float LerpSpeed { get; protected set; }
    public void AssignLerpSpeed(float lerpSpeed)
    {
        LerpSpeed = lerpSpeed;
    }
}