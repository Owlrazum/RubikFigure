using System.Collections;
using System.Collections.Generic;

using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

using Orazum.Utilities;

public class Wheel : MonoBehaviour
{
    [SerializeField]
    private float _testSpeed = 1;

    private UIButton _shuffleButton;
    private WheelSegment[] _segments;

    private int[] _segmentPointsStates;
    private WheelData _data;

    private NativeArray<VertexData> _vertexBuffer;
    private bool _needsDispose;

    private bool _isMoveJobScheduled;

    public void AssignData(WheelData wheelDataArg)
    {
        _data = wheelDataArg;

        // Reverse is needed because math convention of sin and cos is in counter clockwise order,
        // which resulted in segmentPoints ordered in a similar fashion
        // Here the positive means clockwise.
        CollectionsUtilities.ReverseNativeArray<SegmentPoint>(_data.SegmentPoints);

        _segmentPointsStates = new int[_data.SideCount * _data.SegmentCountInOneSide];
        for (int i = 0; i < _segmentPointsStates.Length; i++)
        {
            _segmentPointsStates[i] = i / _data.SegmentCountInOneSide;
        }

        _vertexBuffer = new NativeArray<VertexData>(_data.Vertices, Allocator.Persistent);
        _needsDispose = true;
    }

    public void GenerationInitialization(WheelSegment[] segmentMeshFiltersArg, UIButton shuffleButtonArg)
    {
        _segments = segmentMeshFiltersArg;

        _shuffleButton = shuffleButtonArg;
        _shuffleButton.EventOnTouch += Shuffle;

        _shuffleIndices = new int2[_segments.Length / 2];
        _shuffleSegments = new int[_segments.Length / 2];
        for (int i = 0; i < _segments.Length; i++)
        {
            if (i % 2 == 0)
            { 
                _segments[i].gameObject.SetActive(false);
            }
            else
            {
                int2 index = new int2(i / _data.SegmentCountInOneSide, i % _data.SegmentCountInOneSide);
                index.x = index.x + 1 < _data.SideCount ? index.x + 1 : 0;
                _shuffleIndices[i / 2] = index;
                _shuffleSegments[i / 2] = i;
            }
        }
    }

    private void OnDestroy()
    {
        _shuffleButton.EventOnTouch -= Shuffle;

        if (_needsDispose)
        {
            _vertexBuffer.Dispose();
        }
    }

    private int2[] _shuffleIndices;
    private int[] _shuffleSegments;
    private SegmentMoveType moveType = SegmentMoveType.Clockwise;

    private void Shuffle()
    {
        if (!_isMoveJobScheduled)
        {
            for (int i = 0; i < _shuffleSegments.Length; i++)
            {
                int2 index = _shuffleIndices[i];
                SegmentPoint target = _data.SegmentPoints[
                    index.x * _data.SegmentCountInOneSide +
                    index.y
                ];

                index.x = index.x + 1 < _data.SideCount ? index.x + 1 : 0;
                _shuffleIndices[i] = index;

                _segments[_shuffleSegments[i]].StartSchedulingMoveJobs(
                    target,
                    2,
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
        return y * _data.SegmentCountInOneSide + x;
    }
}