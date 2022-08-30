using UnityEngine.Assertions;
using static Orazum.Constants.Layers;

public class ValknutStatesController : FigureStatesController
{
    public override void Initialize(Figure figure, FigureParamsSO figureParams)
    {
        Valknut valknut = figure as Valknut;
        Assert.IsNotNull(valknut);
        if (figureParams.SelectMethod == SelectMethodType.Raycast)
        {
            RaycastSelectable raycastSelectable = new RaycastSelectable(SegmentPointsLayerMask);
            IdleState = new FigureIdleState(raycastSelectable, this, valknut);
        }
        MoveState = new ValknutMoveState(this, valknut, figureParams.MoveLerpSpeed);
        ShuffleState = new ValknutShuffleState(this, valknut, figureParams);

        _currentState = ShuffleState;
        _currentState.OnEnter();

        StartCoroutine(StateSwitchSequence());
    }
}