using Unity.Mathematics;

public class ValknutStatesController : FigureStatesController
{
    public override void Initialize(Figure figureArg, FigureParamsSO figureParams)
    {
        StartCoroutine(StateSwitchSequence());
    }
}