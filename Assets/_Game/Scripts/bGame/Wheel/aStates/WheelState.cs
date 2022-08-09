public abstract class WheelState
{
    public WheelState(WheelGenerationData generationData)
    { 

    }

    public abstract WheelState HandleTransitions();
    public virtual void OnEnter(Wheel wheel)
    { 

    }

    public virtual void OnExit()
    { 

    }

    public abstract void StartProcessingState(Wheel wheel);
    public abstract void OnDestroy();
    protected abstract WheelState GetThisState();
}