using UnityEngine.Assertions;
using static Orazum.Constants.Layers;

public class ValknutStatesController : FigureStatesController
{
    public override void Initialize(Figure figure, FigureParamsSO figureParams, FigureGenParamsSO genParams)
    {
        Valknut valknut = figure as Valknut;
        Assert.IsNotNull(valknut);
        if (figureParams.SelectMethod == SelectMethodType.Raycast)
        {
            RaycastSelectable raycastSelectable = new RaycastSelectable(SegmentPointsLayerMask);
            IdleState = new FigureIdleState(raycastSelectable, this, valknut);
        }
        else
        {
            throw new System.NotSupportedException("Unknown Select method");
        }
        MoveState = new ValknutMoveState(this, valknut, figureParams.MoveLerpSpeed);
        ShuffleState = new FigureShuffleState(this, valknut, figureParams, genParams);
    }
}