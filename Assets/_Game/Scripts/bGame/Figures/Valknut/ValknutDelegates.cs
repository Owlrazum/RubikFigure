using System;
public static class ValknutDelegates
{
    public static Func<Valknut> GetCurrentWheel;

    public static Action<ValknutSegment[]> EventSegmentsWereEmptied;
    public static Func<FigureSegmentPoint[]> GetEmptySegmentPoints;
    public static Action ActionCheckWheelCompletion;

    public static Action EventWheelWasCompleted;
}