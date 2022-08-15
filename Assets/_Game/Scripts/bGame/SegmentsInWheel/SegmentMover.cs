using System;
using System.Collections;

using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Assertions;

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
    private float _currentSpeed;
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
        float speed,
        Action<int2> OnMoveToDestinationCompleted)
    {
        _currentMove = move;
        _currentSpeed = speed;
        _wasMoveCompleted = false;
        _moveCompleteAction = OnMoveToDestinationCompleted;

        switch (_currentMove.Type)
        { 
            case SegmentMoveType.Down:
            case SegmentMoveType.Up:
                StartCoroutine(MoveSequence());
                break;
            case SegmentMoveType.Clockwise:
            case SegmentMoveType.CounterClockwise:
                StartCoroutine(RotateSequence());
                break;
        }
    }

    private IEnumerator RotateSequence()
    {
        yield break;
        // float lerpParam = 0;
        // float angleDelta = _currentMove.MoveType;
        // while (lerpParam < 1)
        // {
        //     transform.localRotation = Quaternion.AngleAxis(Vector3.up, _currentAngle);
        //     yield return null;
        // }
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
            lerpParam += _currentSpeed * Time.deltaTime;
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

    public void TeleportTo(SegmentPoint destination)
    {
        VertexData data;
        for (int corner = 0; corner < 8; corner++)
        {
            int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner);
            float3 cornerPos = float3.zero;//destination.CornerPositions.GetCornerPosition(corner);

            data = _vertices[meshCorner.x];
            data.position = cornerPos;
            _vertices[meshCorner.x] = data;

            data = _vertices[meshCorner.y];
            data.position = cornerPos;
            _vertices[meshCorner.y] = data;

            data = _vertices[meshCorner.z];
            data.position = cornerPos;
            _vertices[meshCorner.z] = data;
        }

        AssignVertices(_vertices);
    }

    private void AssignVertices(NativeArray<VertexData> toAssign)
    {
        Mesh newMesh = _meshFilter.mesh;
        // newMesh.SetVertexBufferData(toAssign, 0, 0,
        //     VERTEX_COUNT, 0,
        //     MESH_UPDATE_FLAGS
        // );

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