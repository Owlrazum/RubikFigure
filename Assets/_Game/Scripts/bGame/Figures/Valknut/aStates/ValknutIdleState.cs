using UnityEngine;
using UnityEngine.Assertions;

public abstract class ValknutIdleState : FigureIdleState
{
    public override FigureState MoveToAnotherStateOnInput()
    {
        return null;
    }
}