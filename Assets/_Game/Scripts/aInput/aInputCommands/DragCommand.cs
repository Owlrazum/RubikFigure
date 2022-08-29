using Unity.Mathematics;

public class DragCommand : InputCommand
{ 
    public float2 Direction    { get; private set; }
    public float  PercentageToView       { get; private set; }
    public bool   IsCompleted  { get; private set; }

    public DragCommand(float2 directionArg, float percentageToView, bool isCompletedArg)
    {
        Direction = directionArg;
        PercentageToView = percentageToView;
        IsCompleted = isCompletedArg;
    }

    public void UpdateDirection(float2 direction)
    {
        Direction = direction;
    }

    public void UpdatePercentageToView(float value)
    {
        PercentageToView = value;
    }

    public void ConvertToCompleted()
    {
        IsCompleted = true;
    }
}
