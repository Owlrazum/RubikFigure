using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;
using Orazum.Utilities.ConstContainers;

public class Wheel : MonoBehaviour
{
    private int _sideCount;
    private int _segmentCountInOneSide;

    private Array2D<SegmentPoint> _segmentPoints;

    private int2[] _emptySegmentPointIndices;

    private HashSet<SegmentMove> _currentMoves; // the value is whether a move IsCompleted

    public void GenerationInitialization(WheelGenerationData generationData)
    {
        _segmentCountInOneSide = generationData.SegmentCountInOneSide;
        _sideCount = generationData.SideCount;

        // we need to change dimensions from generation
        _segmentPoints = new Array2D<SegmentPoint>(_sideCount, _segmentCountInOneSide);
        GameObject segmentPointsContainerGb = new GameObject("SegmentPoints");
        Transform segmentPointsParent = segmentPointsContainerGb.transform;
        segmentPointsParent.parent = transform;
        segmentPointsParent.SetSiblingIndex(0);
        for (int side = 0; side < _sideCount; side++)
        {
            for (int col = 0; col < _segmentCountInOneSide; col++)
            {
                int cornerIndex = side * _segmentCountInOneSide + col;
                GameObject segmentPointGb = new GameObject("Point[" + side + "," + col + "]");
                segmentPointGb.layer = LayerUtilities.SEGMENT_POINTS_LAYER;
                segmentPointGb.transform.parent = segmentPointsParent;

                SegmentPoint segmentPoint = segmentPointGb.AddComponent<SegmentPoint>();
                segmentPoint.Segment = generationData.Segments[col, side];
                segmentPoint.Initialize(generationData.SegmentPointCornerPositions[cornerIndex],
                    generationData.EmptyMaterial, generationData.HighlightMaterial, new int2(side, col));
                
                _segmentPoints[side, col] = segmentPoint;
            }
        }

        if (generationData.LevelDescriptionSO.ShouldUsePredefinedEmptyPlaces)
        {
            _emptySegmentPointIndices = new int2[generationData.LevelDescriptionSO.PredefinedEmptyPlaces.Length];
            generationData.LevelDescriptionSO.PredefinedEmptyPlaces.CopyTo(_emptySegmentPointIndices, 0);
        }
        else
        {
            GenerateRandomEmptyPoints(generationData.LevelDescriptionSO.EmptyPlacesCount);
        }

        for (int i = 0; i < _emptySegmentPointIndices.Length; i++)
        {
            int2 index = _emptySegmentPointIndices[i];
            print(index);
            _segmentPoints[index].Segment.Dissappear();
            _segmentPoints[index].Segment = null;
        }

        _currentMoves = new HashSet<SegmentMove>(_emptySegmentPointIndices.Length);
    }
    private void GenerateRandomEmptyPoints(int emptyPlacesCount)
    {
        var randomGenerator = Unity.Mathematics.Random.CreateFromIndex(15);
        _emptySegmentPointIndices = new int2[emptyPlacesCount];
        Assert.IsTrue(_emptySegmentPointIndices.Length < _sideCount * _segmentCountInOneSide / 2);
        HashSet<int2> _emptiedSet = new HashSet<int2>();
        for (int i = 0; i < _emptySegmentPointIndices.Length; i++)
        {
            int2 rndIndex = randomGenerator.
                NextInt2(int2.zero, new int2(_sideCount, _segmentCountInOneSide));
            while (_emptiedSet.Contains(rndIndex))
            {
                rndIndex = randomGenerator.
                    NextInt2(int2.zero, new int2(_sideCount, _segmentCountInOneSide));
            }

            _emptiedSet.Add(rndIndex);
            _emptySegmentPointIndices[i] = rndIndex;
        }

        // LogEmtpySegments();
    }

    public int2[] GetEmptyIndices()
    {
        return _emptySegmentPointIndices;
    }

    public void MakeMove(in SegmentMove move, float lerpSpeed)
    {
        print("Making move " + move);
        Assert.IsTrue(move.EmtpyPointIndex >= 0);
        Assert.IsTrue(move.IsValid);

        Segment movedSegment = _segmentPoints[move.FromIndex].Segment;
        Assert.IsNotNull(movedSegment);
        _segmentPoints[move.FromIndex].Segment = null;
        _emptySegmentPointIndices[move.EmtpyPointIndex] = move.FromIndex;


        SegmentPoint target = _segmentPoints[move.ToIndex];
        Assert.IsTrue(target.Segment == null);
        target.Segment = movedSegment;
        move.AssignTarget(target);

        movedSegment.StartProcessingMove(
            move,
            lerpSpeed,
            OnSegmentCompletedMove
        );
        // Debug.Log(_segmentPoints);
        _currentMoves.Add(move);
    }

    [ContextMenu("Print")]
    public void LogThis()
    {
        LogEmtpySegments();
        Debug.Log(_segmentPoints);
        LogCurrentMoves();
    }

    

    private void LogEmtpySegments()
    { 
        string log = "";
        foreach (int2 index in _emptySegmentPointIndices)
        {
            log += index + " ";
        }
        Debug.Log(log);
    }

    private void LogCurrentMoves()
    { 
        string log = "";
        foreach (var entry in _currentMoves)
        {
            log += entry.ToString() + "\n";
        }
        Debug.Log(log);
    }

    private void LateUpdate()
    {
        if (_currentMoves.Count > 0)
        { 
            CompleteCurrentMoveJobs();
        }
    }
    private void CompleteCurrentMoveJobs()
    {
        int completedMovesCount = 0;
        foreach (KeyValuePair<SegmentMove, bool> entry in _currentMoves)
        {
            SegmentMove move = entry.Key;
            Assert.IsTrue(move.IsValid);
            if (_currentMoves[move])
            {
                completedMovesCount++;
            }
            else
            { 
                _segmentPoints[move.ToIndex].Segment.CompleteProcessingMove();
            }
        }

        if (completedMovesCount == _currentMoves.Count)
        {
            foreach (KeyValuePair<SegmentMove, bool> entry in _currentMoves)
            {   
                SegmentMove move = entry.Key;
                _segmentPoints[move.ToIndex].Segment.OnMoveComplete();
            }
            // LogThis();
            _currentMoves.Clear();
            _currentBusySegmentPointIndices.Clear();
        }
    }
    
    private void OnSegmentCompletedMove(SegmentMove move)
    {
        _currentMoves[move] = true;
    }

    public Vector3 GetEmptySegmentPointPosition(int emptyIndex)
    {
        int2 index = _emptySegmentPointIndices[emptyIndex];
        return _segmentPoints[index].transform.position;
    }
    
    public bool IsAdjacentToEmpty(int2 empty, int2 toCheck)
    {
        Assert.IsTrue(math.any(empty != toCheck));
        int2 delta = math.abs(toCheck - empty);
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

    public bool HasSegmentThatWillMoveDown(int2 emptyIndex)
    {
        int2 upIndex = MoveIndexUp(emptyIndex);
        if (upIndex.y >= _segmentCountInOneSide)
        {
            return false;
        }

        bool isNotEmpty = _segmentPoints[upIndex].Segment != null;
        if (isNotEmpty && !_currentBusySegmentPointIndices.Contains(upIndex))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool HasSegmentThatWillMoveUp(int2 emptyIndex)
    {
        int2 downIndex = MoveIndexDown(emptyIndex);
        if (downIndex.y < 0)
        {
            return false;
        }

        bool isNotEmpty = _segmentPoints[downIndex].Segment != null;
        if (isNotEmpty && !_currentBusySegmentPointIndices.Contains(downIndex))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool HasSegmentThatWillMoveCounterClockwise(int2 emptyIndex)
    {
        int2 ccw = MoveIndexClockwise(emptyIndex);
        bool isNotEmpty = _segmentPoints[ccw].Segment != null;
        if (isNotEmpty && !_currentBusySegmentPointIndices.Contains(ccw))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool HasSegmentThatWillMoveClockwise(int2 emptyIndex)
    {
        int2 cw = MoveIndexCounterClockwise(emptyIndex);
        bool isNotEmpty = _segmentPoints[cw].Segment != null;

        if (isNotEmpty && !_currentBusySegmentPointIndices.Contains(cw))
        {
            return true;
        }
        else
        {
            return false;
        }
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

    private int XyToIndex(int x, int y)
    {
        return IndexUtilities.XyToIndex(y, x, _segmentCountInOneSide);
    }
}