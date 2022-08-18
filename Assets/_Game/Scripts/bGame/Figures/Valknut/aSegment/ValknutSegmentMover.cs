using System;
using System.Collections;

using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

using static Orazum.Math.MathUtilities;

[RequireComponent(typeof(MeshFilter))]
public class ValknutSegmentMover : FigureSegmentMover
{ 
    private const float CLOCK_MOVE_BUFFER_LERP_VALUE = 0.4f;

    private bool _wasJobScheduled;
    private bool _wasMoveCompleted;

    public override void StartMove(
        FigureSegmentMove move,
        float lerpSpeed,
        Action OnMoveToDestinationCompleted)
    {
        base.StartMove(move, lerpSpeed, OnMoveToDestinationCompleted);
        _wasMoveCompleted = false;

        if (move is ValknutVerticesMove verticesMove)
        {
            StartCoroutine(MoveSequence(verticesMove));
        }
    }

    private IEnumerator MoveSequence(ValknutVerticesMove verticesMove)
    {
        float lerpParam = 0;
        ValknutSegmentMoveJob segmentMoveJob = new ValknutSegmentMoveJob()
        {
            P_ClockMoveBufferLerpValue = CLOCK_MOVE_BUFFER_LERP_VALUE,
            P_VertexPositions = verticesMove.VertexPositions,
            P_VertexCountInOneSegment = _vertices.Length,

            InputVertices = _vertices,
            OutputVertices = _currentVertices
        };

        segmentMoveJob.P_VertexPositions = verticesMove.VertexPositions;

        while (lerpParam < 1)
        {
            lerpParam += _currentLerpSpeed * Time.deltaTime;
            if (lerpParam > 1)
            {
                lerpParam = 1;
            }
            segmentMoveJob.P_LerpParam = EaseInOut(lerpParam);
            _segmentMoveJobHandle = segmentMoveJob.Schedule(_segmentMoveJobHandle);
            _wasJobScheduled = true;
            yield return null;
        }

        _wasMoveCompleted = true;
    }

    private void LateUpdate()
    {
        if (_wasMoveCompleted)
        {
            _segmentMoveJobHandle.Complete();
            AssignVertices(_currentVertices, _vertices.Length);

            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = _currentVertices[i];
            }
            _moveCompleteAction?.Invoke();
            _wasMoveCompleted = false;
        }
        else if (_wasJobScheduled)
        {
            _segmentMoveJobHandle.Complete();
            AssignVertices(_currentVertices, _vertices.Length);
            
            _wasJobScheduled = false;
        }
    }
}