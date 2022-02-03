using UnityEngine;

public class DragCommand : InputCommand
{ 
    public Vector2 Direction    { get; private set; }
    public float   Amount       { get; private set; }
    public bool    IsCompleted  { get; private set; }

    public DragCommand(Vector2 directionArg, float amountArg, bool isCompletedArg)
    {
        Direction = directionArg;
        Amount = amountArg;
        IsCompleted = isCompletedArg;
    }

    public void ConvertToCompleted()
    {
        IsCompleted = true;
    }
}