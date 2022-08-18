using Unity.Mathematics;

public class ValknutStatesController : FigureStatesController
{
    public override void Initialize(Figure figureArg, FigureParamsSO figureParams, int2[] emptyIndices)
    {
        StartCoroutine(StateSwitchSequence());
    }
}