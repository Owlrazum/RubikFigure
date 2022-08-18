using Unity.Mathematics;
using UnityEngine.Assertions;

public class WheelStatesController : FigureStatesController
{
    private Wheel _wheel;

    public override void Initialize(Figure figureArg, FigureParamsSO figureParams, int2[] emptyIndices)
    {
        _wheel = figureArg as Wheel;
        Assert.IsNotNull(_wheel);

        WheelIdleState idleState = new WheelIdleState();
        WheelMoveState moveState = new WheelMoveState(figureParams.MoveLerpSpeed, _wheel);
        WheelShuffleState shuffleState = new WheelShuffleState(figureParams, _wheel);
        shuffleState.PrepareForShuffle(emptyIndices);

        _currentState = shuffleState;
        _currentState.OnEnter();

        StartCoroutine(StateSwitchSequence());
    }
}