using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;

public class WheelCompleter : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Optimization")]
    private int _maxEmptyPlacesCount;

    [SerializeField]
    private float s_amplitude = 3;

    [SerializeField]
    private float _teleportLerpSpeed = 1;

    private Dictionary<int, List<WheelSegmentMover>> _completionSegmentMovers;
    private Dictionary<int, List<int2>> _potentialAssembleData;

    private List<int2> _emptyIndices; // avoid gc

    private Wheel _currentWheel;
    private void Awake()
    {
        _completionSegmentMovers = new Dictionary<int, List<WheelSegmentMover>>(_maxEmptyPlacesCount);
        _potentialAssembleData = new Dictionary<int, List<int2>>(_maxEmptyPlacesCount);
        _emptyIndices = new List<int2>(_maxEmptyPlacesCount);

        WheelDelegates.EventSegmentsWereEmptied += OnSegmentsWereEmptied;
        WheelDelegates.ActionCheckWheelCompletion += CheckCompletion;
    }

    private void Start()
    {
        _currentWheel = WheelDelegates.GetCurrentWheel();
    }

    private void OnDestroy()
    { 
        WheelDelegates.EventSegmentsWereEmptied -= OnSegmentsWereEmptied;
        WheelDelegates.ActionCheckWheelCompletion -= CheckCompletion;
    }

    private void OnSegmentsWereEmptied(WheelSegment[] segmentsArg)
    {
        _completionSegmentMovers.Clear();

        for (int i = 0; i < segmentsArg.Length; i++)
        {
            int puzzleIndex = segmentsArg[i].PuzzleIndex;
            if (_completionSegmentMovers.ContainsKey(puzzleIndex))
            {
                _completionSegmentMovers[puzzleIndex].Add(segmentsArg[i].GetSegmentMoverForTeleport());
            }
            else
            {
                _completionSegmentMovers.Add(puzzleIndex, new List<WheelSegmentMover>(3));
                _completionSegmentMovers[puzzleIndex].Add(segmentsArg[i].GetSegmentMoverForTeleport());
            }
        }
    }

    private void CheckCompletion()
    {
        Array2D<FigureSegmentPoint> segmentPoints = _currentWheel.GetSegmentPointsForCompletionCheck();
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
        Complete();
    }

    private void Complete()
    {
        WheelDelegates.EventWheelWasCompleted?.Invoke();

        List<WheelTeleportMove> teleportMoves = new List<WheelTeleportMove>(_potentialAssembleData.Count * 2);
        foreach (var entry in _potentialAssembleData)
        {
            int puzzleIndex = entry.Key;

            List<int2> teleportLocationIndices = entry.Value;
            List<WheelSegmentMover> segmentMovers = _completionSegmentMovers[puzzleIndex];
            for (int i = 0; i < teleportLocationIndices.Count; i++)
            {
                Assert.IsNotNull(segmentMovers[i]);
                WheelTeleportMove teleportMove = new WheelTeleportMove();
                teleportMove.AssignSegmentMover(segmentMovers[i]);

                int2 destinationIndex = teleportLocationIndices[i];
                teleportMove.AssignToIndex(destinationIndex);
                teleportMoves.Add(teleportMove);
                segmentMovers[i].Appear();
            }
        }
        _currentWheel.MakeTeleportMoves(teleportMoves, _teleportLerpSpeed);
        StartCoroutine(CompletionSequence(1.0f / _teleportLerpSpeed));
    }

    private IEnumerator CompletionSequence(float beforeRotatePauseTime)
    {
        yield return new WaitForSeconds(beforeRotatePauseTime);
        FigureDelegatesContainer.FigureCompleted?.Invoke();
        Vector3 rotationEuler = Vector3.zero;
        while (true)
        {
            _currentWheel.transform.Rotate(rotationEuler, Space.World);
            rotationEuler.x = s_amplitude * 2 * Time.deltaTime;
            rotationEuler.y = s_amplitude * Time.deltaTime;
            rotationEuler.z = s_amplitude * Time.deltaTime;
            yield return null;
        }
    }
} 