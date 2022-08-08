public class MoveState : WheelState
{
    private float _moveLerpSpeed;

    public MoveState(WheelGenerationData generationData) : base(generationData)
    { 
        _moveLerpSpeed = generationData.LevelDescriptionSO.MoveLerpSpeed;

        WheelStatesDelegates.MoveState += GetThisState;
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
        WheelStatesDelegates.MoveState -= GetThisState;
    }

    protected override WheelState GetThisState()
    {
        return this;
    }
}