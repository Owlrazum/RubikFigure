using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;

public class FigurePuzzler : MonoBehaviour
{
    private FigureParamsSO _figureParams;
    private FigureGenParamsSO _genParams;

    // Assuming always by column, puzzleIndex should be equal column-wise
    // Currently, only byColumn is implemented
    private enum PuzzleType
    {
        ByColumn,
        ByRow
    }

    private Dictionary<int, List<FigureSegment>> _emptiedSegmentsByPuzzleIndex;
    private Dictionary<int, List<int2>> _assembleIndicesByPuzzleIndex;

    private List<int> _emptyPuzzleIndices;

    private List<int2> _targetIndicesBuffer;

    private Figure _figure;
    private int _emptyPlacesCount;

    protected virtual void Awake()
    {
        FigureDelegatesContainer.ActionGrabParameters += GrapParameters;
        FigureDelegatesContainer.ActionStartPuzzle += StartPuzzle;
        FigureDelegatesContainer.ActionCheckCompletion += CheckCompletion;
    }

    protected virtual void OnDestroy()
    {
        FigureDelegatesContainer.ActionGrabParameters -= GrapParameters;
        FigureDelegatesContainer.ActionStartPuzzle -= StartPuzzle;
        FigureDelegatesContainer.ActionCheckCompletion -= CheckCompletion;
    }

    private void GrapParameters(LevelDescriptionSO levelDescription)
    {
        _figureParams = levelDescription.FigureParams;
        _genParams = levelDescription.GenParams;
        int maxPuzzleIndexCount = _genParams.Dimensions.x;
        _emptiedSegmentsByPuzzleIndex = new Dictionary<int, List<FigureSegment>>(maxPuzzleIndexCount);
        _assembleIndicesByPuzzleIndex = new Dictionary<int, List<int2>>(maxPuzzleIndexCount);
        _targetIndicesBuffer = new List<int2>(_emptyPlacesCount);
        _emptyPuzzleIndices = new List<int>(maxPuzzleIndexCount);

        _emptyPlacesCount = _figureParams.EmptyPlacesCount;
    }

    private void StartPuzzle(Figure figure)
    {
        _figure = figure;
        StartCoroutine(DelayedEmpty());
    }

    private IEnumerator DelayedEmpty()
    {
        yield return new WaitForSeconds(_figureParams.BeforeEmptyTime);
        int2[] emptyIndices = EmptyPoints();
        MakeEmptyMoves(emptyIndices);
    }

    private int2[] EmptyPoints()
    {
        int2[] emptyIndices = null;

        if (_figureParams.ShouldUsePredefinedEmptyPlaces)
        {
            emptyIndices = new int2[_figureParams.PredefinedEmptyPlaces.Length];
            _figureParams.PredefinedEmptyPlaces.CopyTo(emptyIndices, 0);
        }
        else
        {
            int2 dims = _genParams.Dimensions;
            emptyIndices =
                GenerateRandomEmptyPoints(_emptyPlacesCount, dims.x, dims.y);
        }

        return emptyIndices;
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

    private void MakeEmptyMoves(int2[] emptyIndices)
    {
        FigureVerticesMove[] emptyMoves = new FigureVerticesMove[emptyIndices.Length];
        for (int i = 0; i < emptyMoves.Length; i++)
        {
            FigureVerticesMove emptyMove = new FigureVerticesMove();
            emptyMove.AssignFromIndex(emptyIndices[i]);
            emptyMove.AssignLerpSpeed(_figureParams.EmptyLerpSpeed);
            emptyMoves[i] = emptyMove;
        }

        FigureSegment[] emptiedSegments = _figure.EmptyAndGetSegments(emptyMoves, OnFinishedEmptyMoves);
        ConvertToDictionaryByPuzzleIndex(emptiedSegments);
    }

    private void OnFinishedEmptyMoves()
    {
        foreach (var entry in _emptiedSegmentsByPuzzleIndex)
        {
            foreach (var segment in entry.Value)
            {
                segment.Dissappear();
            }
        }

        _figure.StatesController.StartUpdating();
    }

    private void ConvertToDictionaryByPuzzleIndex(FigureSegment[] emptiedSegments)
    {
        for (int i = 0; i < emptiedSegments.Length; i++)
        {
            int puzzleIndex = emptiedSegments[i].PuzzleIndex;
            if (!_emptiedSegmentsByPuzzleIndex.ContainsKey(puzzleIndex))
            {
                _emptiedSegmentsByPuzzleIndex.Add(puzzleIndex, new List<FigureSegment>(_emptyPlacesCount));
            }

            _emptiedSegmentsByPuzzleIndex[puzzleIndex].Add(emptiedSegments[i]);

            if (_emptiedSegmentsByPuzzleIndex[puzzleIndex].Count == _genParams.Dimensions.y)
            {
                _emptyPuzzleIndices.Add(puzzleIndex);
            }
        }
    }

    private void CheckCompletion()
    {
        _assembleIndicesByPuzzleIndex.Clear();
        _targetIndicesBuffer.Clear();
        Array2D<FigureSegmentPoint> segmentPoints = _figure.GetSegmentPointsForCompletionCheck();

        int emptyPuzzleIndexer = 0;
        for (int col = 0; col < segmentPoints.ColCount; col++)
        {
            int puzzleIndex = -1;
            for (int row = 0; row < segmentPoints.RowCount; row++)
            {
                int2 index = new int2(col, row);
                if (segmentPoints[index].Segment == null)
                {
                    _targetIndicesBuffer.Add(index);
                    continue;
                }

                if (puzzleIndex < 0)
                {
                    puzzleIndex = segmentPoints[index].Segment.PuzzleIndex;
                }
                else if (puzzleIndex != segmentPoints[index].Segment.PuzzleIndex)
                {
                    Debug.Log($"NoCompletion: puzzleIndices {puzzleIndex} != {segmentPoints[index].Segment.PuzzleIndex}");
                    return;
                }
            }
            if (puzzleIndex < 0)
            {
                if (_emptyPuzzleIndices.Count == 0)
                {
                    Debug.Log("NoCompletion: no puzzleIndices in the column and no emptyPuzzleIndices");
                    return;
                }

                int emptyPuzzleIndex = _emptyPuzzleIndices[emptyPuzzleIndexer++];
                _assembleIndicesByPuzzleIndex.Add(emptyPuzzleIndex, _targetIndicesBuffer);
            }

            Assert.IsFalse(_assembleIndicesByPuzzleIndex.ContainsKey(puzzleIndex));
            if (_targetIndicesBuffer.Count > 0)
            {
                _assembleIndicesByPuzzleIndex.Add(puzzleIndex, _targetIndicesBuffer);
                _targetIndicesBuffer = new List<int2>(_emptyPlacesCount);
            }
        }
        Complete();
    }

    private void Complete()
    {
        Debug.Log("=== COMPLETED===");
        List<FigureVerticesMove> completionMoves = new List<FigureVerticesMove>(_emptyPlacesCount);
        foreach (var entry in _assembleIndicesByPuzzleIndex)
        {
            int puzzleIndex = entry.Key;

            List<int2> teleportLocationIndices = entry.Value;
            List<FigureSegment> puzzleSegments = _emptiedSegmentsByPuzzleIndex[puzzleIndex];
            for (int i = 0; i < teleportLocationIndices.Count; i++)
            {
                Assert.IsNotNull(puzzleSegments[i]);
                FigureVerticesMove completionMove = new FigureVerticesMove();

                int2 destinationIndex = teleportLocationIndices[i];
                completionMove.AssignToIndex(destinationIndex);
                completionMove.AssignCompletionSegment(puzzleSegments[i]);
                completionMove.AssignLerpSpeed(_figureParams.CompleteLerpSpeed);
                completionMoves.Add(completionMove);
            }
        }
        _figure.StatesController.StopUpdating();
        for (int i = 0; i < completionMoves.Count; i++)
        {
            Debug.Log(completionMoves[i].ToIndex);
        }
        _figure.Complete(completionMoves, FigureDelegatesContainer.Completed);
        StartCoroutine(CompletionSequence(1.0f / _figureParams.CompleteLerpSpeed, _figure.transform));
    }

    private IEnumerator CompletionSequence(float beforeRotatePauseTime, Transform figureTransform)
    {
        yield return new WaitForSeconds(beforeRotatePauseTime);
        Vector3 rotationEuler = Vector3.zero;
        while (true)
        {
            figureTransform.Rotate(rotationEuler, Space.World);
            rotationEuler.x = _figureParams.RotationAmplitude * 2 * Time.deltaTime;
            rotationEuler.y = _figureParams.RotationAmplitude * Time.deltaTime;
            rotationEuler.z = _figureParams.RotationAmplitude * Time.deltaTime;
            yield return null;
        }
    }
}