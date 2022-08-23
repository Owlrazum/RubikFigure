using System;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;
using Orazum.Math;

public abstract class Figure : MonoBehaviour
{
    protected Array2D<FigureSegmentPoint> _segmentPoints;
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
        FigureStatesController statesController,
        FigureParamsSO figureParams
    )
    {
        _segmentPoints = segmentPoints;

        _dims.x = _segmentPoints.ColCount;
        _dims.y = _segmentPoints.RowCount;

        _startTeleportPosition = figureParams.StartPositionForSegmentsInCompletionPhase;

        EmptyPoints(figureParams);
        _movedSegmentsBuffer = new FigureSegment[_dims.x * _dims.y];

        statesController.Initialize(this, figureParams);
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

    public int2 MoveIndexInClockOrder(int2 index, ClockOrder clockOrder)
    {
        switch (clockOrder)
        { 
            case ClockOrder.CW:
                index.x = index.x + 1 < _dims.x ? index.x + 1 : 0;
                return index;
            case ClockOrder.CCW:
                index.x = index.x - 1 >= 0 ? index.x - 1 : _dims.x - 1;
                return index;
        }

        throw new ArgumentException("Unknown clock order type");
    }
    public bool IsValidIndexClockOrder(int2 index, ClockOrder clockOrder)
    { 
        switch (clockOrder)
        {
            case ClockOrder.CW:
                index.x++;
                if (index.x >= _dims.x)
                {
                    return false;
                }

                return IsPointEmpty(index);
            case ClockOrder.CCW:
                index.x--;
                if (index.x < 0)
                {
                    return false;
                }

                return IsPointEmpty(index);
        }

        throw new ArgumentException("Unknown clock order type");
    }
    
    public int2 MoveIndexVertOrder(int2 index, VertOrder vertOrder)
    {
        switch(vertOrder)
        {
            case VertOrder.Up:
                index.y++;
                return index;
            case VertOrder.Down:
                index.y--;
                return index;
        }

        throw new ArgumentException("Unknown vertical order type");
    }
    public bool IsValidIndexVertOrder(int2 index, VertOrder vertOrder)
    {
        switch(vertOrder)
        {
            case VertOrder.Up:
                index.y++;
                if (index.y >= _dims.y)
                {
                    Debug.Log("Index exceed");
                    return false;
                }
                
                bool isEmpty = IsPointEmpty(index);
                return isEmpty;
            case VertOrder.Down:
                index.y--;
                if (index.y < 0)
                {
                    Debug.Log("Index exceed");
                    return false;
                }
                isEmpty = IsPointEmpty(index);
                return isEmpty;
        }
        
        throw new ArgumentException("Unknown vertical order type");
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
                GenerateRandomEmptyPoints(figureParams.EmptyPlacesCount,  dims.x, dims.y);
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
        var randomGenerator = Unity.Mathematics.Random.CreateFromIndex(15);
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

    public override string ToString()
    {
        return $"Figure {GetFigureName()}:\n{_segmentPoints}";
    }

    private string figureName;
    protected abstract string GetFigureName();
}