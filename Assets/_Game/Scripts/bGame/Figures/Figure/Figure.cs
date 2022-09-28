using System;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Meshing;
using Orazum.Collections;
using Orazum.Math;

public abstract class Figure : MonoBehaviour
{
    private FigureStatesController _statesController;
    public FigureStatesController StatesController { get { return _statesController; } }

    protected Array2D<FigureSegmentPoint> _segmentPoints;
    private Array2D<FadeOutInTransitions> _shuffleTransitions;

    protected int2 _dims;
    public int2 Dimensions { get { return _dims; } }
    public int ColCount { get { return _dims.x; } }
    public int RowCount { get { return _dims.y; } }

    private int2 _movesCount;
    public bool IsMakingMoves { get { return _movesCount.x < _movesCount.y; } }
    private Action _movesCompleteAction;

    private FigureSegment[] _movedSegmentsBuffer;

    protected abstract void MakeSegmentMove(FigureSegment segment, FigureSegmentMove move, Action moveCompleteAction);

    public virtual void Initialize(
        Array2D<FigureSegmentPoint> segmentPoints,
        FigureParamsSO figureParams
    )
    {
        _segmentPoints = segmentPoints;
        TryGetComponent(out _statesController);

        _dims.x = _segmentPoints.ColCount;
        _dims.y = _segmentPoints.RowCount;

        _movedSegmentsBuffer = new FigureSegment[_dims.x * _dims.y];

        _statesController.Initialize(this, figureParams);
    }


    public void AssignUniversalTransitions(Array2D<FadeOutInTransitions> transitions)
    {
        _shuffleTransitions = transitions;
    }

    public FigureSegment[] EmptyAndGetSegments(FigureVerticesMove[] emptyMoves, Action emptyCompletedAction)
    {
        _movesCompleteAction = emptyCompletedAction;
        _movesCount = new int2(0, emptyMoves.Length);

        FigureSegment[] emptiedSegments = new FigureSegment[emptyMoves.Length];
        for (int i = 0; i < emptyMoves.Length; i++)
        {
            int2 index = emptyMoves[i].FromIndex;

            Assert.IsNotNull(_segmentPoints[index].Segment);
            emptiedSegments[i] = _segmentPoints[index].Segment;
            _segmentPoints[index].Segment = null;

            MakeUniversalMove(emptiedSegments[i], emptyMoves[i], MoveCompleteAction);
        }
        return emptiedSegments;
    }

    public void Shuffle(IList<FigureVerticesMove> moves, Action movesCompleteAction)
    {
        _movesCompleteAction = movesCompleteAction;
        _movesCount = new int2(0, moves.Count);

        for (int i = 0; i < moves.Count; i++)
        {
            FigureVerticesMove move = moves[i];
            FigureSegment movedSegment = _segmentPoints[move.FromIndex].Segment;
            if (movedSegment == null)
            {
                _movedSegmentsBuffer[i] = null;
                _movesCount.x++;
                continue;
            }

            MakeUniversalMove(movedSegment, move, MoveCompleteAction);

            _movedSegmentsBuffer[i] = movedSegment;
            _segmentPoints[move.FromIndex].Segment = null;
        }

        for (int i = 0; i < moves.Count; i++)
        {
            if (_movedSegmentsBuffer[i] == null)
            {
                continue;
            }
            _segmentPoints[moves[i].ToIndex].Segment = _movedSegmentsBuffer[i];
        }
    }

    public void Complete(IList<FigureVerticesMove> moves, Action movesCompleteAction)
    {
        Debug.Log("Figure making completion moves");
        _movesCompleteAction = movesCompleteAction;
        _movesCount = new int2(0, moves.Count);

        for (int i = 0; i < moves.Count; i++)
        {
            FigureVerticesMove move = moves[i];
            move.CompletionSegment.Appear();
            MakeUniversalMove(move.CompletionSegment, move, MoveCompleteAction);

            _segmentPoints[move.ToIndex].Segment = move.CompletionSegment;
        }
    }

    private void MakeUniversalMove(FigureSegment segment, FigureVerticesMove move, Action moveCompleteAction)
    {
        Assert.IsTrue(IsValidIndex(move.FromIndex) || IsValidIndex(move.ToIndex));
        AssignUniversalTransition(move);


        segment.StartMove(move, moveCompleteAction);
    }

    private void AssignUniversalTransition(FigureVerticesMove move)
    {
        if (move.FromIndex.x < 0 || move.ToIndex.x < 0)
        {
            if (move.FromIndex.x >= 0)
            {
                move.Transition = _shuffleTransitions[move.FromIndex].FadeOut;
            }
            else
            {
                move.Transition = _shuffleTransitions[move.ToIndex].FadeIn;
                Debug.Log($"FadeIn assign to {move.ToIndex} {move.Transition}");
            }
        }
        else
        { 
            QS_Transition fadeOut = _shuffleTransitions[move.FromIndex].FadeOut;
            QS_Transition fadeIn = _shuffleTransitions[move.ToIndex].FadeIn;
            var buffer = QS_Transition.PrepareConcatenationBuffer(fadeOut, fadeIn, Allocator.Persistent);
            QS_Transition concShuffle = QS_Transition.Concatenate(fadeOut, fadeIn, buffer);
            move.ShouldDisposeTransition = true;
            move.Transition = concShuffle;
        }
    }

    public void MakeMoves(IList<FigureSegmentMove> moves, Action movesCompleteAction)
    {
        _movesCompleteAction = movesCompleteAction;
        _movesCount = new int2(0, moves.Count);

        for (int i = 0; i < moves.Count; i++)
        {
            FigureSegmentMove move = moves[i];
            FigureSegment movedSegment = _segmentPoints[move.FromIndex].Segment;
            if (movedSegment == null)
            {
                _movedSegmentsBuffer[i] = null;
                _movesCount.x++;
                continue;
            }

            MakeSegmentMove(movedSegment, move, MoveCompleteAction);

            _movedSegmentsBuffer[i] = movedSegment;
            _segmentPoints[move.FromIndex].Segment = null;
        }

        for (int i = 0; i < moves.Count; i++)
        {
            if (_movedSegmentsBuffer[i] == null)
            {
                continue;
            }
            _segmentPoints[moves[i].ToIndex].Segment = _movedSegmentsBuffer[i];
        }
    }
    private void MoveCompleteAction()
    {
        _movesCount.x++;
        Assert.IsTrue(_movesCount.x <= _movesCount.y);
        if (_movesCount.x == _movesCount.y)
        {
            _movesCompleteAction?.Invoke();
        }
    }

    public Array2D<FigureSegmentPoint> GetSegmentPointsForCompletionCheck()
    {
        return _segmentPoints;
    }

    public int2 MoveIndexInClockOrder(int2 index, ClockOrderType clockOrder)
    {
        return MoveIndexClockOrder(index, clockOrder, _dims);
    }
    public bool IsValidIndexClockOrder(int2 index, ClockOrderType clockOrder)
    {
        return IsOutOfDimsClockOrder(index, clockOrder, _dims);
    }

    public int2 MoveIndexVertOrder(int2 index, VertOrderType vertOrder)
    {
        return MoveIndexVertOrder(index, vertOrder, _dims);
    }
    public bool IsOutOfDimsVertOrder(int2 index, VertOrderType vertOrder)
    {
        return IsOutOfDimsVertOrder(index, vertOrder, _dims);
    }

    public bool IsValidIndex(int2 index)
    {
        if (index.y >= 0 && index.y < _dims.y && index.x >= 0 && index.x < _dims.x)
        {
            return true;
        }

        return false;
    }
    public bool IsPointEmpty(int2 index)
    {
        return _segmentPoints[index].Segment == null;
    }

    public static int2 MoveIndexClockOrder(int2 index, ClockOrderType clockOrder, int2 dims)
    {
        if (clockOrder == ClockOrderType.CW)
        {
            index.x = index.x + 1 < dims.x ? index.x + 1 : 0;
            return index;
        }
        else
        {
            index.x = index.x - 1 >= 0 ? index.x - 1 : dims.x - 1;
            return index;
        }
    }
    public static int2 MoveIndexVertOrder(int2 index, VertOrderType vertOrder, int2 dims)
    {
        if (vertOrder == VertOrderType.Up)
        {
            index.y = index.y + 1 < dims.y ? index.y + 1 : 0;
            return index;
        }
        else
        {
            index.y = index.y - 1 >= 0 ? index.y - 1 : dims.y - 1;
            return index;
        }
    }

    public static bool IsOutOfDimsClockOrder(int2 index, ClockOrderType clockOrder, int2 dims)
    {
        if (clockOrder == ClockOrderType.CW)
        {
            index.x++;
            if (index.x >= dims.x)
            {
                return false;
            }

            return true;
        }
        else
        {
            index.x--;
            if (index.x < 0)
            {
                return false;
            }

            return true;
        }
    }
    
    public static bool IsOutOfDimsVertOrder(int2 index, VertOrderType vertOrder, int2 dims)
    {
        if (vertOrder == VertOrderType.Up)
        {
            index.y++;
            if (index.y >= dims.y)
            {
                return false;
            }

            return true;
        }
        else
        {
            index.y--;
            if (index.y < 0)
            {
                return false;
            }

            return true;
        }
    }

    public override string ToString()
    {
        return $"Figure {GetFigureName()}:\n{_segmentPoints}";
    }

    private string figureName;
    protected abstract string GetFigureName();
}