using System;
using System.Collections;

using Unity.Jobs;
using Unity.Mathematics;

using UnityEngine;

using Orazum.Meshing;
using static Orazum.Math.MathUtilities;

[RequireComponent(typeof(MeshFilter))]
public class WheelSegmentMover : FigureSegmentMover
{ 
    private const float ClockMoveBufferLerpValue = 0.4f;

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
            float3 targetPos = vertexPositions.GetPointVertexPos(i);

            data = _vertices[i];
            data.position = targetPos;
            _vertices[i] = data;
        }

        AssignVertices(_vertices, _vertices.Length);
    }

    private IEnumerator MoveSequence(WheelVerticesMove verticesMove)
    {
        float lerpParam = 0;
        WheelSegmentMoveJob segmentMoveJob = new WheelSegmentMoveJob()
        {
            P_ClockMoveBufferLerpValue = ClockMoveBufferLerpValue,
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