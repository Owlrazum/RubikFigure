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

    protected override void MakeSegmentMove(FigureSegment segment, FigureSegmentMove move, Action moveCompleteAction)
    {
        Assert.IsTrue(IsValidIndex(move.FromIndex) && IsValidIndex(move.ToIndex));
        if (move is FigureVerticesMove verticesMove)
        { 
            // Assert.IsNull(_segmentPoints[move.ToIndex].Segment);
            // Assert.IsNotNull(_segmentPoints[move.FromIndex].Segment);
            AssignTransitionData(verticesMove);
        }
        else
        {
            throw new System.ArgumentException("Unknown type of move");
        }

        segment.StartMove(move, moveCompleteAction);
    }

    private void AssignTransitionData(FigureVerticesMove verticesMove)
    {
        int2 from = verticesMove.FromIndex;
        int2 to = verticesMove.ToIndex;
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