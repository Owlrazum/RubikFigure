using System;

public static class WheelStatesDelegates
{
    public static Func<WheelState> IdleState;
    public static Func<WheelState> ShuffleState;
    public static Func<WheelState> MoveState;
}