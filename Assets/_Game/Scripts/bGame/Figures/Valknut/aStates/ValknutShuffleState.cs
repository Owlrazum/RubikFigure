public class ValknutShuffleState : FigureShuffleState
{
    private Valknut _valknut;
    public ValknutShuffleState(ValknutStatesController statesController, Valknut valknut, FigureParamsSO figureParams)
        : base(statesController, valknut, figureParams) 
    {
        _valknut = valknut;
    }

    protected override FigureSegmentMove[] Shuffle(float lerpSpeed)
    {
        return base.Shuffle(lerpSpeed);
    }

    protected override void ShuffleIndices()
    {
                
    }
}