using System;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;
using Orazum.Utilities.ConstContainers;
using static Orazum.Math.MathUtilities;

public class Wheel : MonoBehaviour
{
    private int _sideCount;
    private int _ringCount;

    public int SideCount { get { return _sideCount; } }
    public int RingCount { get { return _ringCount; } }

    private Array2D<SegmentPoint> _segmentPoints;
    private SegmentVertexPositions[] _segmentVertexPositions;

    private HashSet<int2> _currentMovesDestinations; // used by shuffleState
    private Action _currentMoveCompleteAction; // used by moveState

    private void Awake()
    {
        WheelDelegates.GetCurrentWheel += GetThis;
    }
    private void OnDestroy()
    { 
        WheelDelegates.GetCurrentWheel -= GetThis;
    }
    private Wheel GetThis()
    {
        return this;
    }

    public void GenerationInitialization(WheelGenerationData generationData)
    {
        _ringCount = generationData.RingCount;
        _sideCount = generationData.SideCount;
        _segmentPoints = generationData.SegmentPoints;
        _segmentVertexPositions = generationData.SegmentVertexPositions;

        RotateSegmentOnGeneration();

        int2[] emptySegmentPointIndices = null;

        if (generationData.LevelDescription.ShouldUsePredefinedEmptyPlaces)
        {
            emptySegmentPointIndices = new int2[generationData.LevelDescription.PredefinedEmptyPlaces.Length];
            generationData.LevelDescription.PredefinedEmptyPlaces.CopyTo(emptySegmentPointIndices, 0);
        }
        else
        {
            emptySegmentPointIndices = 
                GenerateRandomEmptyPoints(generationData.LevelDescription.EmptyPlacesCount);
        }

        Segment[] emptySegments = new Segment[emptySegmentPointIndices.Length];
        for (int i = 0; i < emptySegmentPointIndices.Length; i++)
        {
            int2 index = emptySegmentPointIndices[i];
            emptySegments[i] = _segmentPoints[index].Segment;
            _segmentPoints[index].Segment.Dissappear();
            _segmentPoints[index].Segment = null;
        }
        WheelDelegates.EventSegmentsWereEmptied?.Invoke(emptySegments);

        _currentMovesDestinations = new HashSet<int2>(emptySegmentPointIndices.Length);

        generationData.EmtpySegmentPointIndicesForShuffle = emptySegmentPointIndices;
    }

    private void RotateSegmentOnGeneration()
    {
        for (int side = 0; side < _sideCount; side++)
        {
            float rotationAngle = side * TAU / _sideCount * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.AngleAxis(rotationAngle, Vector3.up);
            for (int ring = 0; ring < _ringCount; ring++)
            {
                int2 index = new int2(side, ring);
                _segmentPoints[index].Segment.transform.localRotation = rotation;
                _segmentPoints[index].transform.localRotation = rotation;
            }
        }
    }
    
    private int2[] GenerateRandomEmptyPoints(int emptyPlacesCount)
    {
        var randomGenerator = Unity.Mathematics.Random.CreateFromIndex(15);
        int2[] emptySegmentPointIndices = new int2[emptyPlacesCount];
        Assert.IsTrue(emptySegmentPointIndices.Length <= (_sideCount * _ringCount) / 2);
        HashSet<int2> _emptiedSet = new HashSet<int2>();
        for (int i = 0; i < emptySegmentPointIndices.Length; i++)
        {
            int2 rndIndex = randomGenerator.
                NextInt2(int2.zero, new int2(_sideCount, _ringCount));
            while (_emptiedSet.Contains(rndIndex))
            {
                rndIndex = randomGenerator.
                    NextInt2(int2.zero, new int2(_sideCount, _ringCount));
            }

            _emptiedSet.Add(rndIndex);
            emptySegmentPointIndices[i] = rndIndex;
        }

        return emptySegmentPointIndices;
    }

    public void MakeMove(in SegmentMove move, float lerpSpeed, Action moveCompleteAction = null)
    {
        _currentMoveCompleteAction = moveCompleteAction;

        print("Making move " + move);
        Segment movedSegment = _segmentPoints[move.FromIndex].Segment;
        print(move.FromIndex + " " + movedSegment);
        Assert.IsNotNull(movedSegment);
        _segmentPoints[move.FromIndex].Segment = null;

        SegmentPoint target = _segmentPoints[move.ToIndex];
        target.Segment = movedSegment;
        if (move.Type == SegmentMoveType.Down || move.Type == SegmentMoveType.Up)
        { 
            move.AssignVertexPositions(_segmentVertexPositions[move.ToIndex.y]);
        }

        movedSegment.StartMove(
            move,
            lerpSpeed,
            OnSegmentCompletedMove
        );
        _currentMovesDestinations.Add(move.ToIndex);
    }

    public void MakeMoveCollection(SegmentMove[] moves, float lerpSpeed, Action moveCompleteAction = null)
    {
        Segment[] movedSegments = new Segment[moves.Length];
        for (int i = 0; i < moves.Length; i++)
        {
            SegmentMove move = moves[i];
            Segment movedSegment = _segmentPoints[move.FromIndex].Segment;
            if (movedSegment == null)
            {
                continue;
            }
            Assert.IsTrue(move.Type != SegmentMoveType.Down && move.Type != SegmentMoveType.Up);
            movedSegment.StartMove(
                move,
                lerpSpeed,
                null
            );

            movedSegments[i] = movedSegment;
            _segmentPoints[move.FromIndex].Segment = null;
        }

        for (int i = 0; i < moves.Length; i++)
        {
            if (movedSegments[i] == null)
            {
                continue;
            }
            SegmentMove move = moves[i];
            _segmentPoints[move.ToIndex].Segment = movedSegments[i];
        }
    }

    private void OnSegmentCompletedMove(int2 destination)
    {
        Assert.IsTrue(_currentMovesDestinations.Contains(destination));
        _currentMovesDestinations.Remove(destination);
        _currentMoveCompleteAction?.Invoke();
    }

    public Array2D<SegmentPoint> GetSegmentPointsForCompletionCheck()
    {
        return _segmentPoints;
    }

    public Vector3 GetEmptySegmentPointPosition(int2 emptyIndex)
    {
        return _segmentPoints[emptyIndex].transform.position;
    }
    public SegmentPoint GetSegmentPointForTeleport(int2 index)
    {
        return _segmentPoints[index];
    }

    public bool IsIndexAdjacentTo(int2 lhs, int2 rhs)
    {
        Assert.IsTrue(math.any(lhs != rhs));
        int2 delta = math.abs(rhs - lhs);
        if (delta.x == 0)
        {
            if (delta.y == 1)
            {
                return true;
            }
        }
        else if (delta.y == 0)
        {
            if (delta.x == _sideCount - 1 || delta.x == 1)
            {
                return true;
            }
        }
        
        return false;
    }
    public bool IsPointEmpty(int2 index)
    {
        return _segmentPoints[index].Segment == null;
    }
    public bool DoesIndexHaveAdjacentEmptyIndex(int2 index)
    {
        int2 check = MoveIndexClockwise(index);
        if (IsPointEmpty(check))
        {
            return true;
        }

        check = MoveIndexCounterClockwise(index);
        if (IsPointEmpty(check))
        {
            return true;
        }

        check = MoveIndexDown(index);
        if (check.y >= 0 && IsPointEmpty(check))
        {
            return true;
        }

        check = MoveIndexUp(index);
        if(check.y < _ringCount && IsPointEmpty(check))
        {
            return true;
        }

        return false;
    }

    public void DeterminePossibleMoves(int2 emptyIndex, List<SegmentMove> possibleMoves)
    {
        if (HasSegmentThatWillMoveDown(emptyIndex))
        {
            possibleMoves.Add(new SegmentMove(SegmentMoveType.Down, MoveIndexUp(emptyIndex), emptyIndex));
        }
        if (HasSegmentThatWillMoveUp(emptyIndex))
        {
            possibleMoves.Add(new SegmentMove(SegmentMoveType.Up, MoveIndexDown(emptyIndex), emptyIndex));
        }
        if (HasSegmentThatWillMoveCounterClockwise(emptyIndex))
        {
            possibleMoves.Add(
                new SegmentMove(SegmentMoveType.CounterClockwise, 
                MoveIndexClockwise(emptyIndex), emptyIndex)
            );
        }
        if (HasSegmentThatWillMoveClockwise(emptyIndex))
        {
            possibleMoves.Add(
                new SegmentMove(SegmentMoveType.Clockwise, 
                MoveIndexCounterClockwise(emptyIndex), emptyIndex));
        }
    }
    public bool IsMovePossible(SegmentMove move, out int2 toIndex)
    {
        Assert.IsNotNull(_segmentPoints[move.FromIndex].Segment);
        toIndex = int2.zero;
        switch (move.Type)
        {
            case SegmentMoveType.Down:
                if (CanMoveDown(move.FromIndex))
                {
                    toIndex = MoveIndexDown(move.FromIndex);
                    return true;
                }
                break;
            case SegmentMoveType.Up:
                if (CanMoveUp(move.FromIndex))
                {
                    toIndex = MoveIndexUp(move.FromIndex);
                    return true;
                }
                break;
            case SegmentMoveType.CounterClockwise:
                if (CanMoveCounterClockwise(move.FromIndex))
                {
                    toIndex = MoveIndexCounterClockwise(move.FromIndex);
                    return true;
                }
                break;
            case SegmentMoveType.Clockwise:
                if (CanMoveClockwise(move.FromIndex))
                {
                    toIndex = MoveIndexClockwise(move.FromIndex);
                    return true;
                }
                break;
        }

        return false;
    }

    private bool HasSegmentThatWillMoveDown(int2 emptyIndex)
    {
        if (!IsValidIndexForMoveUp(emptyIndex))
        {
            return false;
        }

        int2 upIndex = MoveIndexUp(emptyIndex);
        if (IsPointFreeToMoveInto(upIndex))
        {
            return false;
        }

        return true;
    }
    private bool HasSegmentThatWillMoveUp(int2 emptyIndex)
    {
        if (!IsValidIndexForMoveDown(emptyIndex))
        {
            return false;
        }

        int2 downIndex = MoveIndexDown(emptyIndex);
        if (IsPointFreeToMoveInto(downIndex))
        {
            return false;
        }

         return true;
    }
    private bool HasSegmentThatWillMoveCounterClockwise(int2 emptyIndex)
    {
        int2 ccw = MoveIndexClockwise(emptyIndex);
        if (IsPointFreeToMoveInto(ccw))
        {
            return false;
        }

        return true;
    }
    private bool HasSegmentThatWillMoveClockwise(int2 emptyIndex)
    {
        int2 cw = MoveIndexCounterClockwise(emptyIndex);
        if (IsPointFreeToMoveInto(cw))
        {
            return false;
        }
         
        return true;
    }

    public int2 MoveIndexDown(int2 index)
    {
        index.y--;
        return index;
    }
    public int2 MoveIndexUp(int2 index)
    {
        index.y++;
        return index;
    }
    public int2 MoveIndexCounterClockwise(int2 index)
    {
        index.x = index.x - 1 >= 0 ? index.x - 1 : _sideCount - 1;
        return index;
    }
    public int2 MoveIndexClockwise(int2 index)
    {
        index.x = index.x + 1 < _sideCount ? index.x + 1 : 0;
        return index;
    }

    private bool CanMoveDown(int2 index)
    {
        if (!IsValidIndexForMoveDown(index))
        {
            return false;
        }

        int2 downIndex = MoveIndexDown(index);
        return IsPointFreeToMoveInto(downIndex);
    }
    private bool CanMoveUp(int2 index)
    {
        if (!IsValidIndexForMoveUp(index))
        {
            return false;
        }

        int2 upIndex = MoveIndexUp(index);
        return IsPointFreeToMoveInto(upIndex);
    }
    private bool CanMoveCounterClockwise(int2 index)
    {
        int2 ccw = MoveIndexCounterClockwise(index);
        return IsPointFreeToMoveInto(ccw);
    }
    private bool CanMoveClockwise(int2 index)
    {
        int2 cw = MoveIndexClockwise(index);
        return IsPointFreeToMoveInto(cw);
    }

    private bool IsValidIndexForMoveDown(int2 index)
    {
        if (index.y - 1 >= 0)
        {
            return true;
        }
        return false;
    }
    private bool IsValidIndexForMoveUp(int2 index)
    {   
        if (index.y + 1 < _ringCount)
        {
            return true;
        }
        return false;
    }

    private bool IsPointFreeToMoveInto(int2 pointIndex)
    {
        return IsPointEmpty(pointIndex) || _currentMovesDestinations.Contains(pointIndex);
    }

    private int XyToIndex(int x, int y)
    {
        return IndexUtilities.XyToIndex(y, x, _ringCount);
    }

    [ContextMenu("Print")]
    public void LogThis()
    {
        Debug.Log(_segmentPoints);
        LogCurrentMoves();
    }
    private void LogCurrentMoves()
    { 
        string log = "";
        foreach (var entry in _currentMovesDestinations)
        {
            log += entry.ToString() + "\n";
        }
        Debug.Log(log);
    }
}