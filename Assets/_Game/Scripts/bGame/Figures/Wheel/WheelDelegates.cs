using System;

public static class WheelDelegates
{
    public static Func<Wheel> GetCurrentWheel;

    public static Action<WheelSegment[]> EventSegmentsWereEmptied;
    public static Func<FigureSegmentPoint[]> GetEmptySegmentPoints;
    public static Action ActionCheckWheelCompletion;
}