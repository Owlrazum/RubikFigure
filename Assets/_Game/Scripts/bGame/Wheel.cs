using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

public class Wheel : MonoBehaviour
{
    private float _wheelLerpSpeed = 1;
    private float _wheelPauseTime = 0.2f;

    private NativeArray<VertexData> _vertices;

    private WheelSegment[] _segments;
    private NativeArray<SegmentPoint> _segmentPoints;
    private int[] _segmentPointsStates;

    private int _sideCount;
    private int _segmentCountInOneSide;

    private bool _isMoveJobScheduled;

    public void GenerationInitialization(WheelGenerationData generationData)
    {
        _vertices = generationData.Vertices;
        _segmentPoints = generationData.SegmentPoints;

        _segments = generationData.Segments;
        _segmentCountInOneSide = generationData.SegmentCountInOneSide;
        _sideCount = generationData.SideCount;

        _shuffleIndices = new int2[_segments.Length];
        _shuffleSegments = new int[_segments.Length];
        for (int i = 0; i < _segments.Length; i++)
        {
            int2 index = new int2(i / _segmentCountInOneSide, i % _segmentCountInOneSide);
            _shuffleIndices[i] = index;
            _shuffleSegments[i] = i;
        }

        _segmentPointsStates = new int[_sideCount * _segmentCountInOneSide];
        for (int i = 0; i < _segmentPointsStates.Length; i++)
        {
            _segmentPointsStates[i] = i / _segmentCountInOneSide;
        }

        _wheelLerpSpeed = GameDelegatesContainer.GetWheelLerpSpeed();

        StartCoroutine(ShuffleSequence());
    }

    private void OnDestroy()
    {
    }

    private IEnumerator ShuffleSequence()
    {
        while (true)
        {
            Shuffle();
            yield return new WaitForSeconds(1 / _wheelLerpSpeed + _wheelPauseTime);
        }
    }

    private int2[] _shuffleIndices;
    private int[] _shuffleSegments;

    private void Shuffle()
    {
        if (!_isMoveJobScheduled)
        {
            for (int i = 0; i < _shuffleSegments.Length; i++)
            {
                int2 index = _shuffleIndices[i];
                SegmentMoveType moveType;
                if (index.y % 2 == 1)
                {
                    moveType = SegmentMoveType.CounterClockwise;
                    index.x = index.x - 1 >= 0 ? index.x - 1 : _sideCount - 1;
                }
                else
                { 
                    moveType = SegmentMoveType.Clockwise;
                    // index.x = index.x - 1 >= 0 ? index.x - 1 : _sideCount - 1;
                    index.x = index.x + 1 < _sideCount ? index.x + 1 : 0;
                }
                _shuffleIndices[i] = index;

                SegmentPoint target = _segmentPoints[
                    index.x * _segmentCountInOneSide +
                    index.y
                ];
                
                _segments[_shuffleSegments[i]].StartSchedulingMoveJobs(
                    target,
                    _wheelLerpSpeed,
                    moveType,
                    OnCompleteMoveJobSchedule
                );
            }
            _isMoveJobScheduled = true;
        }
    }

    private void OnCompleteMoveJobSchedule(int index)
    {
        _segments[index].CompleteMoveJob();
        _isMoveJobScheduled = false;
    }

    private void LateUpdate()
    {
        if (_isMoveJobScheduled)
        {
            for (int i = 0; i < _shuffleSegments.Length; i++)
            { 
                _segments[_shuffleSegments[i]].CompleteMoveJob();
            }
        }
    }

    private int Index(int x, int y)
    {
        return y * _segmentCountInOneSide + x;
    }
}