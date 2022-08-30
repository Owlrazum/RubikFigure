using UnityEngine.Assertions;
using static Orazum.Constants.Layers;

public class WheelStatesController : FigureStatesController
{
    public override void Initialize(Figure figure, FigureParamsSO figureParams)
    {
        Wheel wheel = figure as Wheel;
        Assert.IsNotNull(wheel);

        if (figureParams.SelectMethod == SelectMethodType.Raycast)
        {
            RaycastSelectable raycastSelectable = new RaycastSelectable(SegmentPointsLayerMask);
            IdleState = new FigureIdleState(raycastSelectable, this, wheel);
        }

        MoveState = new WheelMoveState(this, wheel, figureParams.MoveLerpSpeed);
        ShuffleState = new WheelShuffleState(this, wheel, figureParams);

        _currentState = ShuffleState;
        _currentState.OnEnter();

        StartCoroutine(StateSwitchSequence());
    }
}