public class WheelIdleState : FigureIdleState
{
    public WheelIdleState(WheelStatesController statesController, Wheel wheel) : base (statesController, wheel)
    { 

    }
    public override string ToString()
    {
        return "WheelIdleState";
    }
}