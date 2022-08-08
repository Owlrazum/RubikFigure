using Unity.Mathematics;

public class SegmentPoint
{
    public Segment Segment { get; set; }

    private SegmentPointCornerPositions _cornerPositions;
    public SegmentPoint(SegmentPointCornerPositions cornerPositionsArg)
    {
        _cornerPositions = cornerPositionsArg;
    }

    public SegmentPointCornerPositions GetCornerPositions()
    {
        return _cornerPositions;
    }

    public override string ToString()
    {
        if (Segment == null)
        { 
            return "0";
        }
        else
        {
            return "1";
        }
    }
}