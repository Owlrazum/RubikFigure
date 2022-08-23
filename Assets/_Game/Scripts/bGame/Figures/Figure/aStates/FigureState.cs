public abstract class FigureState
{
    protected Figure _figure;
    protected FigureStatesController _statesController;
    public FigureState(FigureStatesController statesController, Figure figure)
    {
        _statesController = statesController;
        _figure = figure;
    }

    public abstract FigureState HandleTransitions();
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void ProcessState() { }
}