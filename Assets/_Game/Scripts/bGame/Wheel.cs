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

        _shuffleIndices = new int2[_segments.Length];
        _shuffleSegments = new int[_segments.Length];
        for (int i = 0; i < _segments.Length; i++)
        {
            int2 index = new int2(i / _data.SegmentCountInOneSide, i % _data.SegmentCountInOneSide);
            _shuffleIndices[i] = index;
            _shuffleSegments[i] = i;
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
                    index.x = index.x - 1 >= 0 ? index.x - 1 : _data.SideCount - 1;
                }
                else
                { 
                    moveType = SegmentMoveType.Clockwise;
                    // index.x = index.x - 1 >= 0 ? index.x - 1 : _data.SideCount - 1;
                    index.x = index.x + 1 < _data.SideCount ? index.x + 1 : 0;
                }
                _shuffleIndices[i] = index;

                SegmentPoint target = _data.SegmentPoints[
                    index.x * _data.SegmentCountInOneSide +
                    index.y
                ];
                
                _segments[_shuffleSegments[i]].StartSchedulingMoveJobs(
                    target,
                    1,
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