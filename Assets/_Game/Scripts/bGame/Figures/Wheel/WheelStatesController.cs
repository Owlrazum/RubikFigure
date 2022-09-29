using UnityEngine.Assertions;
using static Orazum.Constants.Layers;

public class WheelStatesController : FigureStatesController
{
    public override void Initialize(Figure figure, FigureParamsSO figureParams, FigureGenParamsSO genParams)
    {
        Wheel wheel = figure as Wheel;
        Assert.IsNotNull(wheel);

        if (figureParams.SelectMethod == SelectMethodType.Raycast)
        {
            RaycastSelectable raycastSelectable = new RaycastSelectable(SegmentPointsLayerMask);
            IdleState = new FigureIdleState(raycastSelectable, this, wheel);
            IdleState.SetSelectionScaling(figureParams.SelectionScaling);
        }

        MoveState = new WheelMoveState(this, wheel, figureParams.MoveLerpSpeed);
        ShuffleState = new FigureShuffleState(this, wheel, figureParams, genParams);
    }
}