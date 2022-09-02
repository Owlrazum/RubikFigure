using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;

public class FigureCompleter : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Optimization")]
    protected int _maxEmptyPlacesCount;

    [SerializeField]
    protected float s_amplitude = 3;

    [SerializeField]
    protected float _teleportLerpSpeed = 1;

    protected Dictionary<int, List<FigureSegmentMover>> _completionSegmentMovers;
    protected Dictionary<int, List<int2>> _potentialAssembleData;
    protected List<int2> _emptyIndices; // avoid gc

    protected virtual void Awake()
    {
        _completionSegmentMovers = new Dictionary<int, List<FigureSegmentMover>>(_maxEmptyPlacesCount);
        _potentialAssembleData = new Dictionary<int, List<int2>>(_maxEmptyPlacesCount);
        _emptyIndices = new List<int2>(_maxEmptyPlacesCount);

        FigureDelegatesContainer.EventSegmentsWereEmptied += OnSegmentsWereEmptied;
        FigureDelegatesContainer.ActionCheckCompletion += CheckCompletion;
    }

    protected virtual void OnDestroy()
    {
        FigureDelegatesContainer.EventSegmentsWereEmptied -= OnSegmentsWereEmptied;
        FigureDelegatesContainer.ActionCheckCompletion -= CheckCompletion;
    }

    private void OnSegmentsWereEmptied(FigureSegment[] segmentsArg)
    {
        _completionSegmentMovers.Clear();

        for (int i = 0; i < segmentsArg.Length; i++)
        {
            int puzzleIndex = segmentsArg[i].PuzzleIndex;
            if (_completionSegmentMovers.ContainsKey(puzzleIndex))
            {
                _completionSegmentMovers[puzzleIndex].Add(segmentsArg[i].Mover);
            }
            else
            {
                _completionSegmentMovers.Add(puzzleIndex, new List<FigureSegmentMover>(3));
                _completionSegmentMovers[puzzleIndex].Add(segmentsArg[i].Mover);
            }
        }
    }

    private void CheckCompletion(Figure figure)
    {
        Array2D<FigureSegmentPoint> segmentPoints = figure.GetSegmentPointsForCompletionCheck();
        for (int side = 0; side < segmentPoints.ColCount; side++)
        {
            int puzzleIndex = -1;
            _emptyIndices.Clear();
            for (int ring = 0; ring < segmentPoints.RowCount; ring++)
            {
                int2 index = new int2(side, ring);
                if (segmentPoints[index].Segment == null)
                {
                    _emptyIndices.Add(index);
                    continue;
                }

                if (puzzleIndex < 0)
                {
                    puzzleIndex = segmentPoints[index].Segment.PuzzleIndex;
                }
                else if (puzzleIndex != segmentPoints[index].Segment.PuzzleIndex)
                {
                    _potentialAssembleData.Clear();
                    return;
                }
            }
            if (_emptyIndices.Count > 0)
            {
                _potentialAssembleData.Add(puzzleIndex, _emptyIndices);
                _emptyIndices = new List<int2>(_maxEmptyPlacesCount);
            }
        }
        Complete(figure);
    }

    protected virtual List<FigureSegmentMove> Complete(Figure figure)
    {
        List<FigureSegmentMove> completionMoves = new List<FigureSegmentMove>(_potentialAssembleData.Count * 2);
        foreach (var entry in _potentialAssembleData)
        {
            int puzzleIndex = entry.Key;

            List<int2> teleportLocationIndices = entry.Value;
            List<FigureSegmentMover> segmentMovers = _completionSegmentMovers[puzzleIndex];
            for (int i = 0; i < teleportLocationIndices.Count; i++)
            {
                Assert.IsNotNull(segmentMovers[i]);
                FigureSegmentMove completionMove = new FigureSegmentMove();
                completionMove.AssignMover(segmentMovers[i]);

                int2 destinationIndex = teleportLocationIndices[i];
                completionMove.AssignToIndex(destinationIndex);
                completionMoves.Add(completionMove);
                segmentMovers[i].Appear();
            }
        }

        FigureDelegatesContainer.Completed?.Invoke();
        return completionMoves;
    }
}