public class IdleState : WheelState
{
    public IdleState(WheelGenerationData generationData) : base(generationData)
    { 
        WheelStatesDelegates.IdleState += GetThisState;
    }

    public override WheelState HandleTransitions()
    {
        return this;
    }

    public override void StartProcessingState(Wheel wheel)
    {

    }

    public override void OnDestroy()
    {
        WheelStatesDelegates.IdleState -= GetThisState;
    }

    protected override WheelState GetThisState()
    {
        return this;
    }
}