public abstract class WheelState
{
    protected Wheel _wheel;
    public WheelState(FigureParamsSO figureParams, Wheel wheelArg)
    {
        _wheel = wheelArg;
    }

    public abstract WheelState HandleTransitions();
    public virtual void OnEnter()
    { 

    }

    public virtual void OnExit()
    { 

    }

    public abstract void ProcessState();
    public abstract void OnDestroy();
    protected abstract WheelState GetThisState();
}