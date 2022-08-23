using UnityEngine.Assertions;

public class WheelStatesController : FigureStatesController
{
    private Wheel _wheel;

    public override void Initialize(Figure figureArg, FigureParamsSO figureParams)
    {
        _wheel = figureArg as Wheel;
        Assert.IsNotNull(_wheel);

        IdleState = new WheelIdleState(this, _wheel);
        MoveState = new WheelMoveState(this, _wheel, figureParams.MoveLerpSpeed);
        ShuffleState = new WheelShuffleState(this, _wheel, figureParams);

        _currentState = ShuffleState;
        _currentState.OnEnter();

        StartCoroutine(StateSwitchSequence());
    }
}