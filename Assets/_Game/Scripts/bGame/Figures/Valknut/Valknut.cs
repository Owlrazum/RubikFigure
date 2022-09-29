using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;

public class Valknut : Figure
{
    public const int TrianglesCount = 3;
    public const int PartsCount = 2;

    private Array2D<ValknutSegmentTransitions> _transitions;

    public void AssignTransitionDatas(Array2D<ValknutSegmentTransitions> transitions)
    {
        _transitions = transitions;
    }

    protected override void MakeSegmentMove(FigureSegment segment, FSMC_Transition move, Action moveCompleteAction)
    {
        AssignTransitionData(move);
        segment.StartMove(move, moveCompleteAction);
    }

    private void AssignTransitionData(FSMC_Transition verticesMove)
    {
        int2 from = verticesMove.From;
        int2 to = verticesMove.To;
        if (from.y == 0 && to.y == 0 ||
            from.y == 1 && to.y == 1
        )
        {
            Debug.Log($"VT: CW {from} {to}");
            verticesMove.Transition = _transitions.GetElementByRef(to).CW;
        }
        else
        { 
            Debug.Log($"VT: AntiCW {from} {to}");
            verticesMove.Transition = _transitions.GetElementByRef(to).AntiCW;
        }
    }

    protected override string GetFigureName()
    {
        return "Valknut";
    }
}