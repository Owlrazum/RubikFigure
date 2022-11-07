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

    protected Array2D<FS_Point> _segmentPoints;
    private Array2D<OutInTransitions> _universalTransitions;

    protected int2 _dims;
    public int2 Dimensions { get { return _dims; } }
    public int ColCount { get { return _dims.x; } }
    public int RowCount { get { return _dims.y; } }

    private int2 _movesCount;
    public bool IsMakingMoves { get { return _movesCount.x < _movesCount.y; } }
    private Action _movesCompleteAction;

    private FigureSegment[] _movedSegmentsBuffer;

    protected abstract void MakeSegmentMove(FigureSegment segment, FMSC_Transition move, Action moveCompleteAction);

    public virtual void Initialize(
        Array2D<FS_Point> segmentPoints,
        FigureParamsSO figureParams,
        FigureGenParamsSO genParams
    )
    {
        _segmentPoints = segmentPoints;
        TryGetComponent(out _statesController);

        _dims.x = _segmentPoints.ColCount;
        _dims.y = _segmentPoints.RowCount;

        _movedSegmentsBuffer = new FigureSegment[_dims.x * _dims.y];

        _statesController.Initialize(this, figureParams, genParams);
    }


    public void AssignUniversalTransitions(Array2D<OutInTransitions> transitions)
    {
        Assert.IsTrue(math.all(_dims == new int2(transitions.ColCount, transitions.RowCount)));
        _universalTransitions = transitions;
    }

    public FigureSegment[] EmptyAndGetSegments(FMST_Empty[] emptyMoves, Action emptyCompletedAction)
    {
        _movesCompleteAction = emptyCompletedAction;
        _movesCount = new int2(0, emptyMoves.Length);

        FigureSegment[] emptiedSegments = new FigureSegment[emptyMoves.Length];
        for (int i = 0; i < emptyMoves.Length; i++)
        {
            int2 index = emptyMoves[i].Index;

            Assert.IsNotNull(_segmentPoints[index].Segment);
            emptiedSegments[i] = _segmentPoints[index].Segment;
            _segmentPoints[index].Segment = null;

            MakeUniversalMove(emptiedSegments[i], emptyMoves[i], MoveCompleteAction);
        }
        return emptiedSegments;
    }

    private bool IsUniversalMove(FigureMoveOnSegment move)
    {
        return move is FMST_Empty || move is FMST_Completion || move is FMSCT_Shuffle;
    }
    private void MakeUniversalMove(FigureSegment segment, FigureMoveOnSegment move, Action moveCompleteAction)
    {
        AssignUniversalTransition(move);

        if (segment.gameObject.activeSelf != true)
        {
            segment.gameObject.SetActive(true);
            Debug.Log("Enabling segment gb");
        }
        segment.StartMove(move, moveCompleteAction);
    }


    private void AssignUniversalTransition(FigureMoveOnSegment move)
    {
        switch (move)
        { 
            case FMST_Empty emptyMove:
                int2 index = emptyMove.Index;
                emptyMove.Transition = _universalTransitions[index].Out;
                break;
            case FMST_Completion completionMove:
                index = completionMove.CompletionIndex;
                completionMove.Transition = _universalTransitions[index].In;
                break;
            case FMSCT_Shuffle shuffleMove:
                QS_Transition fadeOut = _universalTransitions[shuffleMove.From].Out;
                QS_Transition fadeIn = _universalTransitions[shuffleMove.To].In;
                var buffer = QS_Transition.PrepareConcatenationBuffer(fadeOut, fadeIn, Allocator.Persistent);
                QS_Transition concShuffle = QS_Transition.Concatenate(fadeOut, fadeIn, buffer);
                shuffleMove.ShouldDisposeTransition = true;
                shuffleMove.Transition = concShuffle;
                break;
            default:
                throw new ArgumentException("Unknown universal move");
        }
    }

    public void MakeMoves(IList<FigureMoveOnSegment> moves, Action movesCompleteAction)
    {
        _movesCompleteAction = movesCompleteAction;
        _movesCount = new int2(0, moves.Count);

        for (int i = 0; i < moves.Count; i++)
        {
            FigureMoveOnSegment move = moves[i];
            FigureSegment movedSegment = GetSegmentToMove(move);
            if (movedSegment == null)
            {
                _movedSegmentsBuffer[i] = null;
                _movesCount.x++;
                continue;
            }

            if (IsUniversalMove(move))
            {
                MakeUniversalMove(movedSegment, move, MoveCompleteAction);
            }
            else
            {
                FMSC_Transition segmentMove = move as FMSC_Transition;
                Assert.IsNotNull(segmentMove, $"move is not indexChange with transition move: {move}");
                MakeSegmentMove(movedSegment, segmentMove, MoveCompleteAction);
            }

            if (move is FMS_IndexChange indexChange)
            {
                _movedSegmentsBuffer[i] = movedSegment;
                _segmentPoints[indexChange.From].Segment = null;
            }
            else
            {
                _movedSegmentsBuffer[i] = null;
            }
        }

        for (int i = 0; i < moves.Count; i++)
        {
            if (_movedSegmentsBuffer[i] == null)
            {
                continue;
            }
            FMS_IndexChange indexChange = moves[i] as FMS_IndexChange;
            Assert.IsNotNull(indexChange);
            _segmentPoints[indexChange.To].Segment = _movedSegmentsBuffer[i];
        }
    }
    private FigureSegment GetSegmentToMove(FigureMoveOnSegment move)
    {
        switch (move)
        { 
            case FMST_Completion completionMove:
                return completionMove.CompletionSegment;
            default:
                return _segmentPoints[move.Index].Segment;
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

    public Array2D<FS_Point> GetSegmentPointsForCompletionCheck()
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