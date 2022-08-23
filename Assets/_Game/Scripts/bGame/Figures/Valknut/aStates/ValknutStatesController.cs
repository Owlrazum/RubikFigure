public class ValknutStatesController : FigureStatesController
{
    public override void Initialize(Figure figure, FigureParamsSO figureParams)
    {
        Valknut valknut = figure as Valknut;
        IdleState = new FigureIdleState(this, valknut);
        MoveState = new ValknutMoveState(this, valknut, figureParams.MoveLerpSpeed);
        ShuffleState = new ValknutShuffleState(this, valknut, figureParams);

        _currentState = ShuffleState;
        _currentState.OnEnter();

        StartCoroutine(StateSwitchSequence());
    }
}