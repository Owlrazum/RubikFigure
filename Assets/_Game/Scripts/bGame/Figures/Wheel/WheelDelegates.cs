using System;

public static class WheelDelegates
{
    public static Func<Wheel> GetCurrentWheel;

    public static Func<WheelIdleState> IdleState;
    public static Func<WheelShuffleState> ShuffleState;
    public static Func<WheelMoveState> MoveState;

    public static Action<WheelSegment[]> EventSegmentsWereEmptied;
    public static Func<FigureSegmentPoint[]> GetEmptySegmentPoints;
    public static Action ActionCheckWheelCompletion;

    public static Action EventWheelWasCompleted;
}