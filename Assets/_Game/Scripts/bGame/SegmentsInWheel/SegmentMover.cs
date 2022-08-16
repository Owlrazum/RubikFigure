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
public class SegmentMover : MonoBehaviour
{ 
    public const MeshUpdateFlags MESH_UPDATE_FLAGS = MeshUpdateFlags.Default;

    private const float CLOCK_MOVE_BUFFER_LERP_VALUE = 0.4f;

    private MeshFilter _meshFilter;

    public MeshFilter MeshContainer { get { return _meshFilter; } }

    private float _currentAngle;

    private NativeArray<VertexData> _vertices;

    private SegmentMoveJob _segmentMoveJob;
    private JobHandle _segmentMoveJobHandle;

    private NativeArray<VertexData> _currentVertices;
    private float _currentLerpSpeed;
    private SegmentMove _currentMove;

    private bool _wasJobScheduled;
    private bool _wasMoveCompleted;
    private Action<int2> _moveCompleteAction;

    private void Awake()
    {
        _currentAngle = 0;
        TryGetComponent(out _meshFilter);
    }

    public void Initialize(NativeArray<VertexData> verticesArg)
    {
        _vertices = verticesArg;

        _currentVertices =
            new NativeArray<VertexData>(_vertices.Length, Allocator.Persistent);
    }

    public void StartMove(
        SegmentMove move,
        float _lerpSpeed,
        Action<int2> OnMoveToDestinationCompleted)
    {
        _currentMove = move;
        _currentLerpSpeed = _lerpSpeed;
        _wasMoveCompleted = false;
        _moveCompleteAction = OnMoveToDestinationCompleted;

        switch (_currentMove.Type)
        { 
            case SegmentMoveType.Clockwise:
            case SegmentMoveType.CounterClockwise:
                StartCoroutine(RotateSequence());
                break;
            case SegmentMoveType.Down:
            case SegmentMoveType.Up:
                StartCoroutine(MoveSequence());
                break;
        }
    }

    private IEnumerator RotateSequence()
    {
        float lerpParam = 0;
        Quaternion initialRotation = transform.localRotation;
        print($"move rotation {_currentMove.rotation.eulerAngles}");
        Quaternion targetRotation = initialRotation * _currentMove.rotation;
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
        print($"{targetRotation.eulerAngles}");

        _moveCompleteAction?.Invoke(_currentMove.ToIndex);
    }

    private IEnumerator MoveSequence()
    {
        float lerpParam = 0;
        _segmentMoveJob = new SegmentMoveJob()
        {
            P_ClockMoveBufferLerpValue = CLOCK_MOVE_BUFFER_LERP_VALUE,
            P_SegmentMoveType = _currentMove.Type,
            P_VertexPositions = _currentMove.VertexPositions,
            P_VertexCountInOneSegment = Segment.VertexCount,

            InputVertices = _vertices,
            OutputVertices = _currentVertices
        };

        if (_currentMove.Type == SegmentMoveType.Down || _currentMove.Type == SegmentMoveType.Up)
        {
            _segmentMoveJob.P_VertexPositions = _currentMove.VertexPositions;
        }

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
            AssignVertices(_currentVertices);

            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = _currentVertices[i];
            }
            _moveCompleteAction?.Invoke(_currentMove.ToIndex);
            _wasMoveCompleted = false;
        }
        else if (_wasJobScheduled)
        {
            _segmentMoveJobHandle.Complete();
            AssignVertices(_currentVertices);
            
            _wasJobScheduled = false;
        }
    }

    private void AssignVertices(NativeArray<VertexData> toAssign)
    {
        Mesh newMesh = _meshFilter.mesh;
        newMesh.SetVertexBufferData(toAssign, 0, 0,
            Segment.VertexCount, 0,
            MESH_UPDATE_FLAGS
        );

        newMesh.RecalculateNormals();
        _meshFilter.mesh = newMesh;
    }

    private void OnDestroy()
    {
        if (!_segmentMoveJobHandle.IsCompleted)
        {
            _segmentMoveJobHandle.Complete();
        }

        CollectionUtilities.DisposeIfNeeded(_currentVertices);
        CollectionUtilities.DisposeIfNeeded(_vertices);
    }
}