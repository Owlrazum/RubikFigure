using System;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;
using static Orazum.Math.MathUtilities;

public class Wheel : MonoBehaviour
{
    private int _sideCount;
    private int _ringCount;

    public int SideCount { get { return _sideCount; } }
    public int RingCount { get { return _ringCount; } }

    private Array2D<SegmentPoint> _segmentPoints;
    private SegmentVertexPositions[] _segmentVertexPositions;

    private Vector3 _startTeleportPosition;

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

        _startTeleportPosition = generationData.LevelDescription.StartPositionForSegmentsInCompletionPhase;

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

        generationData.EmtpySegmentPointIndicesForShuffle = emptySegmentPointIndices;
    }
    private void RotateSegmentOnGeneration()
    {
        for (int side = 0; side < _sideCount; side++)
        {
            for (int ring = 0; ring < _ringCount; ring++)
            {
                Quaternion rotation = GetSideRotation(side);
                int2 index = new int2(side, ring);
                _segmentPoints[index].Segment.transform.localRotation = rotation;
                _segmentPoints[index].transform.localRotation = rotation;
            }
        }
    }
    private Quaternion GetSideRotation(int2 index)
    {
        return GetSideRotation(index.x);
    }
    private Quaternion GetSideRotation(int sideIndex)
    { 
        float rotationAngle = sideIndex * TAU / _sideCount * Mathf.Rad2Deg;
        return Quaternion.AngleAxis(rotationAngle, Vector3.up);
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

    public void MakeVerticesMove(in VerticesMove move, float lerpSpeed, Action moveCompleteAction = null)
    {
        Assert.IsNull(_segmentPoints[move.ToIndex].Segment);
        Assert.IsNotNull(_segmentPoints[move.FromIndex].Segment);
        Segment movedSegment = _segmentPoints[move.FromIndex].Segment;
        _segmentPoints[move.FromIndex].Segment = null;
        _segmentPoints[move.ToIndex].Segment = movedSegment;
        Debug.Log($"Swapped segmentPoint contents {move.FromIndex} {move.ToIndex}");

        move.AssignVertexPositions(GetVertexPositions(move.ToIndex));

        movedSegment.StartMove(
            move,
            lerpSpeed,
            moveCompleteAction
        );
    }

    public void MakeRotationMoves(List<RotationMove> moves, float lerpSpeed, Action moveCompleteAction = null)
    {
        Segment[] movedSegments = new Segment[moves.Count];
        bool isAssignedMoveCompleteAction = false;
        for (int i = 0; i < moves.Count; i++)
        {
            RotationMove rotationMove = moves[i];
            Assert.IsTrue(IsValidIndex(rotationMove.FromIndex) && IsValidIndex(rotationMove.ToIndex));
            float rotationAngle = TAU / _sideCount * Mathf.Rad2Deg;
            if (rotationMove.Type == RotationMove.TypeType.CounterClockwise)
            {
                rotationAngle = -rotationAngle;
            }
            rotationMove.AssignRotation(Quaternion.AngleAxis(rotationAngle, Vector3.up));
            Segment movedSegment = _segmentPoints[rotationMove.FromIndex].Segment;
            Assert.IsNotNull(movedSegment);
            movedSegment.StartMove(
                rotationMove,
                lerpSpeed,
                isAssignedMoveCompleteAction ? null : moveCompleteAction 
            );

            movedSegments[i] = movedSegment;
            _segmentPoints[rotationMove.FromIndex].Segment = null;   
        }

        for (int i = 0; i < moves.Count; i++)
        {
            if (movedSegments[i] == null)
            {
                continue;
            }
            RotationMove move = moves[i];
            _segmentPoints[move.ToIndex].Segment = movedSegments[i];
        }
    }

    public void MakeShuffleMoves(RotationMove[] moves, float lerpSpeed)
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
            RotationMove move = moves[i];
            _segmentPoints[move.ToIndex].Segment = movedSegments[i];
        }
    }
    public void MakeTeleportMoves(List<TeleportMove> moves, float lerpSpeed)
    { 
        for (int i = 0; i < moves.Count; i++)
        {
            TeleportMove teleportMove = moves[i];
            Quaternion rotation = GetSideRotation(teleportMove.ToIndex);
            teleportMove.AssignTargetOrientation(rotation);
            teleportMove.AssignStartTeleportPosition(_startTeleportPosition);
            teleportMove.AssignVertexPositions(GetVertexPositions(teleportMove.ToIndex));
            teleportMove.SegmentMover.StartMove(
                teleportMove,
                lerpSpeed,
                null
            );
        }
    }
    private SegmentVertexPositions GetVertexPositions(int2 index)
    {
        return _segmentVertexPositions[index.y];
    }

    public Array2D<SegmentPoint> GetSegmentPointsForCompletionCheck()
    {
        Debug.Log("returning segmentPoints for completion check");
        return _segmentPoints;
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
        if (IsValidIndexForMoveDown(index) && IsPointEmpty(check))
        {
            return true;
        }

        check = MoveIndexUp(index);
        if (IsValidIndexForMoveUp(index) && IsPointEmpty(check))
        {
            return true;
        }

        return false;
    }

    public void DeterminePossibleMoves(int2 emptyIndex, List<SegmentMove> possibleMoves)
    {
        if (HasSegmentThatWillMoveDown(emptyIndex))
        {
            VerticesMove verticesMove = new VerticesMove();
            verticesMove.AssignType(VerticesMove.TypeType.Down);
            verticesMove.AssignFromIndex(MoveIndexUp(emptyIndex));
            verticesMove.AssignToIndex(emptyIndex);
            possibleMoves.Add(verticesMove);
        }
        if (HasSegmentThatWillMoveUp(emptyIndex))
        {
            VerticesMove verticesMove = new VerticesMove();
            verticesMove.AssignType(VerticesMove.TypeType.Up);
            verticesMove.AssignFromIndex(MoveIndexDown(emptyIndex));
            verticesMove.AssignToIndex(emptyIndex);
            possibleMoves.Add(verticesMove);
        }
        if (HasSegmentThatWillMoveCounterClockwise(emptyIndex))
        {
            RotationMove rotationMove = new RotationMove();
            rotationMove.AssignType(RotationMove.TypeType.CounterClockwise);
            rotationMove.AssignFromIndex(MoveIndexClockwise(emptyIndex));
            rotationMove.AssignToIndex(emptyIndex);
            possibleMoves.Add(rotationMove);
        }
        if (HasSegmentThatWillMoveClockwise(emptyIndex))
        {
            RotationMove rotationMove = new RotationMove();
            rotationMove.AssignType(RotationMove.TypeType.Clockwise);
            rotationMove.AssignFromIndex(MoveIndexCounterClockwise(emptyIndex));
            rotationMove.AssignToIndex(emptyIndex);
            possibleMoves.Add(rotationMove);
        }
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

    public bool IsMovePossibleFromIndex(VerticesMove verticesMove, out int2 toIndex)
    {
        Assert.IsNotNull(_segmentPoints[verticesMove.FromIndex].Segment);
        Assert.IsTrue(IsValidIndex(verticesMove.FromIndex));
        toIndex = int2.zero;
        switch (verticesMove.Type)
        {
            case VerticesMove.TypeType.Down:
                if (CanMoveDown(verticesMove.FromIndex))
                {
                    toIndex = MoveIndexDown(verticesMove.FromIndex);
                    return true;
                }
                break;
            case VerticesMove.TypeType.Up:
                if (CanMoveUp(verticesMove.FromIndex))
                {
                    toIndex = MoveIndexUp(verticesMove.FromIndex);
                    return true;
                }
                break;
        }
        return false;
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

    private bool IsPointFreeToMoveInto(int2 pointIndex)
    {
        return IsPointEmpty(pointIndex);
    }

    private int2 MoveIndexDown(int2 index)
    {
        index.y--;
        return index;
    }
    private int2 MoveIndexUp(int2 index)
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
    private bool IsValidIndex(int2 index)
    {
        if (index.y >= 0 && index.y < _ringCount && index.x >= 0 && index.x < _sideCount)
        {
            return true;
        }

        return false;
    }

    private int XyToIndex(int x, int y)
    {
        return IndexUtilities.XyToIndex(y, x, _ringCount);
    }

    [ContextMenu("Print")]
    public override string ToString()
    {
        Debug.Log(_segmentPoints);
        return _segmentPoints.ToString();
    }
}


/*
    public Vector3 GetEmptySegmentPointPosition(int2 emptyIndex)
    {
        return _segmentPoints[emptyIndex].transform.position;
    }

    
*/