using System;
using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;

public class Valknut : Figure
{
    public const int TrianglesCount = 3;
    public const int TriangleSegmentsCount = 2;

    private Array2D<ValknutQSTransSegs> _transitionDatas;

    public void AssignTransitionDatas(Array2D<ValknutQSTransSegs> transitionDatas)
    {
        _transitionDatas = transitionDatas;
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
        int from = verticesMove.FromIndex.y;
        int to = verticesMove.ToIndex.y;
        if (from == 0 && to == 0 ||
            from == 1 && to == 1
        )
        {
            Assert.IsTrue(_transitionDatas[verticesMove.ToIndex].CW.IsCreated);
            verticesMove.AssignTransitionData(_transitionDatas[verticesMove.ToIndex].CW);
        }
        else
        { 
            Assert.IsTrue(_transitionDatas[verticesMove.ToIndex].AntiCW.IsCreated);
            verticesMove.AssignTransitionData(_transitionDatas[verticesMove.ToIndex].AntiCW);
        }
    }

    protected override string GetFigureName()
    {
        return "Valknut";
    }
}