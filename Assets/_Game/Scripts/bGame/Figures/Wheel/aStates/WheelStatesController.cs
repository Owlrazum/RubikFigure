using UnityEngine.Assertions;

public class WheelStatesController : FigureStatesController
{
    public override void Initialize(Figure figure, FigureParamsSO figureParams)
    {
        Wheel wheel = figure as Wheel;
        Assert.IsNotNull(wheel);

        IdleState = new FigureIdleState(this, wheel);
        MoveState = new WheelMoveState(this, wheel, figureParams.MoveLerpSpeed);
        ShuffleState = new WheelShuffleState(this, wheel, figureParams);

        _currentState = ShuffleState;
        _currentState.OnEnter();

        StartCoroutine(StateSwitchSequence());
    }
}