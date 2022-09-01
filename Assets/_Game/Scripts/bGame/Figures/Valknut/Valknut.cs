using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;

public class Valknut : Figure
{
    public const int TrianglesCount = 3;
    public const int TriangleSegmentsCount = 2;

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
            verticesMove.Transition = ValknutSegmentTransitions.Clockwise(ref _transitions.GetElementByRef(to));
        }
        else
        { 
            verticesMove.Transition = ValknutSegmentTransitions.AntiClockwise(ref _transitions.GetElementByRef(to));
        }
    }

    protected override string GetFigureName()
    {
        return "Valknut";
    }
}