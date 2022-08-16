using System.Collections;
using System.Collections.Generic;

using Unity.Mathematics;
using UnityEngine;

using Orazum.Collections;

public class WheelCompleter : MonoBehaviour
{
    [SerializeField]
    [Tooltip("Optimization")]
    private int _maxEmptyPlacesCount;

    [SerializeField]
    private float s_amplitude = 3;
    private Dictionary<int, List<Segment>> _emptiedSegments;
    private Dictionary<int, List<int2>> _potentialAssembleData;

    private List<int2> _emptyIndices; // avoid gc

    private Wheel _currentWheel;
    private void Awake()
    {
        _emptiedSegments = new Dictionary<int, List<Segment>>(_maxEmptyPlacesCount);
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

    private void OnSegmentsWereEmptied(Segment[] segmentsArg)
    {
        _emptiedSegments.Clear();

        for (int i = 0; i < segmentsArg.Length; i++)
        {
            int puzzleIndex = segmentsArg[i].PuzzleIndex;
            if (_emptiedSegments.ContainsKey(puzzleIndex))
            {
                _emptiedSegments[puzzleIndex].Add(segmentsArg[i]);
            }
            else
            {
                _emptiedSegments.Add(puzzleIndex, new List<Segment>(3));
                _emptiedSegments[puzzleIndex].Add(segmentsArg[i]);
            }
        }
    }

    public void CheckCompletion()
    {
        Array2D<SegmentPoint> segmentPoints = _currentWheel.GetSegmentPointsForCompletionCheck();
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

        foreach (var entry in _potentialAssembleData)
        {
            int puzzleIndex = entry.Key;

            List<int2> teleportLocationIndices = entry.Value;
            List<Segment> toTeleportSegments = _emptiedSegments[puzzleIndex];
            for (int i = 0; i < teleportLocationIndices.Count; i++)
            {
                int2 segmentPointIndex = teleportLocationIndices[i];
                SegmentVertexPositions destination = _currentWheel.GetSegmentPointForTeleport(segmentPointIndex);
                print(toTeleportSegments[i] != null);
                // toTeleportSegments[i].TeleportTo(destination);
                // toTeleportSegments[i].Appear();
            }
        }

        print("Level completed");
        GameDelegatesContainer.EventLevelCompleted?.Invoke();
        StartCoroutine(WheelRotationSequence());
    }

    private IEnumerator WheelRotationSequence()
    {
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