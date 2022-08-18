using System;
using System.Collections;

using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

using Orazum.Collections;
using static Orazum.Math.MathUtilities;

[RequireComponent(typeof(MeshFilter))]
public class WheelSegmentMover : FigureSegmentMover
{ 
    private const float CLOCK_MOVE_BUFFER_LERP_VALUE = 0.4f;

    private float _currentAngle;

    private WheelSegmentMoveJob _segmentMoveJob;

    private bool _wasJobScheduled;
    private bool _wasMoveCompleted;

    public override void StartMove(
        FigureSegmentMove move,
        float lerpSpeed,
        Action OnMoveToDestinationCompleted)
    {
        base.StartMove(move, lerpSpeed, OnMoveToDestinationCompleted);
        _wasMoveCompleted = false;

        if (move is WheelRotationMove rotationMove)
        { 
            StartCoroutine(RotateSequence(rotationMove));
        }
        else if (move is WheelVerticesMove verticesMove)
        {
            StartCoroutine(MoveSequence(verticesMove));
        }
        else if (move is WheelTeleportMove teleportMove)
        {
            StartCoroutine(TeleportSequence(teleportMove));
        }
    }

    private IEnumerator RotateSequence(WheelRotationMove rotationMove)
    {
        float lerpParam = 0;
        Quaternion initialRotation = transform.localRotation;
        Quaternion targetRotation = initialRotation * rotationMove.Rotation;
        while (lerpParam < 1)
        {
            lerpParam += _currentLerpSpeed * Time.deltaTime;
            if (lerpParam > 1)
            {
                lerpParam = 1;
            }
            transform.localRotation = Quaternion.Slerp(initialRotation, targetRotation, EaseInOut(lerpParam));
            yield return null;
        }

        transform.localRotation = targetRotation;

        _moveCompleteAction?.Invoke();
    }

    private IEnumerator TeleportSequence(WheelTeleportMove teleportMove)
    {
        transform.localPosition = teleportMove.StartTeleportPosition;
        transform.localRotation = teleportMove.TargetOrientation;
        TeleportVertices(teleportMove.VertexPositions);

        float lerpParam = 0;
        while (lerpParam < 1)
        {
            lerpParam += _currentLerpSpeed * Time.deltaTime;
            if (lerpParam > 1)
            {
                lerpParam = 1;
            }
            transform.localPosition = Vector3.Lerp(teleportMove.StartTeleportPosition, Vector3.zero, EaseInOut(lerpParam));
            yield return null;
        }
    }

    private void TeleportVertices(WheelSegmentMesh vertexPositions)
    { 
        VertexData data;

        for (int i = 0; i < vertexPositions.Count; i++)
        {
            int2 indices = vertexPositions.GetSegmentIndices(i);
            float3 targetPos = vertexPositions.GetPointVertexPos(i);

            data = _vertices[indices.x];
            data.position = targetPos;
            _vertices[indices.x] = data;
            if (indices.y < 0)
            {
                continue;
            }

            data = _vertices[indices.y];
            data.position = targetPos;
            _vertices[indices.y] = data;
        }

        AssignVertices(_vertices, _vertices.Length);
    }

    private IEnumerator MoveSequence(WheelVerticesMove verticesMove)
    {
        float lerpParam = 0;
        _segmentMoveJob = new WheelSegmentMoveJob()
        {
            P_ClockMoveBufferLerpValue = CLOCK_MOVE_BUFFER_LERP_VALUE,
            P_VertexPositions = verticesMove.VertexPositions,
            P_VertexCountInOneSegment = _vertices.Length,

            InputVertices = _vertices,
            OutputVertices = _currentVertices
        };

        _segmentMoveJob.P_VertexPositions = verticesMove.VertexPositions;

        while (lerpParam < 1)
        {
            lerpParam += _currentLerpSpeed * Time.deltaTime;
            if (lerpParam > 1)
            {
                lerpParam = 1;
            }
            _segmentMoveJob.P_LerpParam = EaseInOut(lerpParam);
            _segmentMoveJobHandle = _segmentMoveJob.Schedule(_segmentMoveJobHandle);
            _wasJobScheduled = true;
            yield return null;
        }

        _wasMoveCompleted = true;
    }

    public void LateUpdate()
    {
        if (_wasMoveCompleted)
        {
            _segmentMoveJobHandle.Complete();
            _currentVertices = _segmentMoveJob.OutputVertices;
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