using Unity.Mathematics;

// FS - Figure Segment
public class FS_Movement
{ 
    public int2 SegmentIndex { get; protected set; }
    public void AssignSegmentIndex(int2 segmentIndex)
    {
        SegmentIndex = segmentIndex;
    }

    public float LerpSpeed { get; protected set; }
    public void AssignLerpSpeed(float lerpSpeed)
    {
        LerpSpeed = lerpSpeed;
    }
}