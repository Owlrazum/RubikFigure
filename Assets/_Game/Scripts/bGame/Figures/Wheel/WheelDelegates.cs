using System;

public static class WheelDelegates
{
    public static Func<Wheel> GetCurrentWheel;

    public static Func<WheelState> IdleState;
    public static Func<WheelState> ShuffleState;
    public static Func<WheelState> MoveState;

    public static Action<Segment[]> EventSegmentsWereEmptied;
    public static Func<SegmentPoint[]> GetEmptySegmentPoints;
    public static Action ActionCheckWheelCompletion;

    public static Action EventWheelWasCompleted;
}