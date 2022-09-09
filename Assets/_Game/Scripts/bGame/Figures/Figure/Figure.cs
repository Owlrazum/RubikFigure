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

    protected Vector3 _startTeleportPosition;

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

        _startTeleportPosition = figureParams.StartPositionForSegmentsInCompletionPhase;

        EmptyPoints(figureParams);
        _movedSegmentsBuffer = new FigureSegment[_dims.x * _dims.y];

        _statesController.Initialize(this, figureParams);
    }

    public void AssignShuffleTransitions(Array2D<FadeOutInTransitions> transitions)
    {
        _shuffleTransitions = transitions;
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
                continue;
            }

            MakeShuffleMove(movedSegment, move, MoveCompleteAction);

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

    private void MakeShuffleMove(FigureSegment segment, FigureVerticesMove move, Action moveCompleteAction)
    {
        Assert.IsTrue(IsValidIndex(move.FromIndex) && IsValidIndex(move.ToIndex));
        AssignShuffleTransitionData(move);

        segment.StartMove(move, moveCompleteAction);
    }

    private void AssignShuffleTransitionData(FigureVerticesMove move)
    {
        QS_Transition fadeOut = _shuffleTransitions[move.FromIndex].FadeOut;
        QS_Transition fadeIn = _shuffleTransitions[move.ToIndex].FadeIn;
        var buffer = QS_Transition.PrepareConcatenationBuffer(fadeOut, fadeIn, Allocator.Persistent);
        QS_Transition concShuffle = QS_Transition.Concatenate(fadeOut, fadeIn, buffer);
        move.Transition = concShuffle;
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


    private void EmptyPoints(FigureParamsSO figureParams)
    {
        int2[] emptySegmentPointIndices = null;

        if (figureParams.ShouldUsePredefinedEmptyPlaces)
        {
            emptySegmentPointIndices = new int2[figureParams.PredefinedEmptyPlaces.Length];
            figureParams.PredefinedEmptyPlaces.CopyTo(emptySegmentPointIndices, 0);
        }
        else
        {
            int2 dims = figureParams.FigureGenParamsSO.Dimensions;
            emptySegmentPointIndices =
                GenerateRandomEmptyPoints(figureParams.EmptyPlacesCount, dims.x, dims.y);
        }

        FigureSegment[] emptySegments = new FigureSegment[emptySegmentPointIndices.Length];
        for (int i = 0; i < emptySegmentPointIndices.Length; i++)
        {
            int2 index = emptySegmentPointIndices[i];
            emptySegments[i] = _segmentPoints[index].Segment;
            Assert.IsNotNull(emptySegments[i]);
            _segmentPoints[index].Segment.Dissappear();
            _segmentPoints[index].Segment = null;
        }
        FigureDelegatesContainer.EventSegmentsWereEmptied?.Invoke(emptySegments);
    }
    private int2[] GenerateRandomEmptyPoints(int emptyPlacesCount, int colCount, int rowCount)
    {
        var randomGenerator = Unity.Mathematics.Random.CreateFromIndex((uint)System.DateTime.Now.Millisecond);
        int2[] emptySegmentPointIndices = new int2[emptyPlacesCount];
        Assert.IsTrue(emptySegmentPointIndices.Length <= (colCount * rowCount) / 2);
        HashSet<int2> _emptiedSet = new HashSet<int2>();
        for (int i = 0; i < emptySegmentPointIndices.Length; i++)
        {
            int2 rndIndex = randomGenerator.
                NextInt2(int2.zero, new int2(colCount, rowCount));
            while (_emptiedSet.Contains(rndIndex))
            {
                rndIndex = randomGenerator.
                    NextInt2(int2.zero, new int2(colCount, rowCount));
            }

            _emptiedSet.Add(rndIndex);
            emptySegmentPointIndices[i] = rndIndex;
        }

        return emptySegmentPointIndices;
    }

    public static int2 MoveIndexClockOrder(int2 index, ClockOrderType clockOrder, int2 dims)
    {
        switch (clockOrder)
        {
            case ClockOrderType.CW:
                index.x = index.x + 1 < dims.x ? index.x + 1 : 0;
                return index;
            case ClockOrderType.AntiCW:
                index.x = index.x - 1 >= 0 ? index.x - 1 : dims.x - 1;
                return index;
        }

        throw new ArgumentException("Unknown clock order type");
    }
    public static int2 MoveIndexVertOrder(int2 index, VertOrderType vertOrder, int2 dims)
    {
        switch (vertOrder)
        {
            case VertOrderType.Up:
                index.y = index.y + 1 < dims.y ? index.y + 1 : 0;
                return index;
            case VertOrderType.Down:
                index.y = index.y - 1 >= 0 ? index.y - 1 : dims.y - 1;
                return index;
        }

        throw new ArgumentException("Unknown vertical order type");
    }

    public static bool IsOutOfDimsClockOrder(int2 index, ClockOrderType clockOrder, int2 dims)
    {
        switch (clockOrder)
        {
            case ClockOrderType.CW:
                index.x++;
                if (index.x >= dims.x)
                {
                    return false;
                }

                return true;
            case ClockOrderType.AntiCW:
                index.x--;
                if (index.x < 0)
                {
                    return false;
                }

                return true;
        }

        throw new ArgumentException("Unknown clock order type");
    }
    public static bool IsOutOfDimsVertOrder(int2 index, VertOrderType vertOrder, int2 dims)
    {
        switch (vertOrder)
        {
            case VertOrderType.Up:
                index.y++;
                if (index.y >= dims.y)
                {
                    return false;
                }

                return true;
            case VertOrderType.Down:
                index.y--;
                if (index.y < 0)
                {
                    return false;
                }

                return true;
        }

        throw new ArgumentException("Unknown vertical order type");
    }

    public override string ToString()
    {
        return $"Figure {GetFigureName()}:\n{_segmentPoints}";
    }

    private string figureName;
    protected abstract string GetFigureName();
}