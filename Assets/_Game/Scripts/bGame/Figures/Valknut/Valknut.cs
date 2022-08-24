using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;
using static Orazum.Math.MathUtilities;

public class Valknut : Figure
{
    public const int TrianglesCount = 3;
    public const int TriangleSegmentsCount = 2;

    private Array2D<ValknutTransitionData> _transitionDatas;

    public void AssignTransitionDatas(Array2D<ValknutTransitionData> transitionDatas)
    {
        _transitionDatas = transitionDatas;
    }

    protected override void MakeSegmentMove(FigureSegment segment, FigureSegmentMove move, Action moveCompleteAction)
    {
        Debug.Log($"Making ${move}");
        Assert.IsTrue(IsValidIndex(move.FromIndex) && IsValidIndex(move.ToIndex));
        if (move is ValknutVerticesMove verticesMove)
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

    private void AssignTransitionData(ValknutVerticesMove verticesMove)
    {
        int from = verticesMove.FromIndex.y;
        int to = verticesMove.ToIndex.y;
        if (from == 0 && to == 0 ||
            from == 1 && to == 1
        )
        {
            Assert.IsTrue(_transitionDatas[verticesMove.ToIndex].PositionsCW.IsCreated);
            Assert.IsTrue(_transitionDatas[verticesMove.ToIndex].LerpRangesCW.IsCreated);
            verticesMove.AssignTransitionPositions(_transitionDatas[verticesMove.ToIndex].PositionsCW);
            verticesMove.AssignTransitionLerpRanges(_transitionDatas[verticesMove.ToIndex].LerpRangesCW);
        }
        else
        { 
            Assert.IsTrue(_transitionDatas[verticesMove.ToIndex].PositionsCCW.IsCreated);
            Assert.IsTrue(_transitionDatas[verticesMove.ToIndex].LerpRangesCCW.IsCreated);
            verticesMove.AssignTransitionPositions(_transitionDatas[verticesMove.ToIndex].PositionsCCW);
            verticesMove.AssignTransitionLerpRanges(_transitionDatas[verticesMove.ToIndex].LerpRangesCCW);
        }
    }

    protected override string GetFigureName()
    {
        return "Valknut";
    }
}