public abstract class FigureState
{
    public abstract FigureState HandleTransitions();
    public virtual void OnEnter() { }
    public virtual void OnExit() { }
    public virtual void ProcessState() { }
    public abstract void OnDestroy();
}