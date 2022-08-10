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
    public const int VERTEX_COUNT = 24;
    public const int INDEX_COUNT = 36;
    public const MeshUpdateFlags MESH_UPDATE_FLAGS = MeshUpdateFlags.Default;

    private const float CLOCK_MOVE_BUFFER_LERP_VALUE = 0.4f;

    private MeshFilter _meshFilter;

    public MeshFilter MeshContainer { get { return _meshFilter; } }

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

        StartCoroutine(MoveSequence());
    }

    private IEnumerator MoveSequence()
    {
        float lerpParam = 0;
        while (lerpParam < 1)
        {
            lerpParam += _currentSpeed * Time.deltaTime;
            if (lerpParam > 1)
            {
                lerpParam = 1;
            }

            _segmentMoveJob = new SegmentMoveJob()
            {
                P_ClockMoveBufferLerpValue = CLOCK_MOVE_BUFFER_LERP_VALUE,
                P_LerpParam = EaseInOut(lerpParam),
                P_SegmentMoveType = _currentMove.MoveType,
                P_SegmentPoint = _currentMove.GetTargetCornerPositions(),
                P_VertexCountInOneSegment = VERTEX_COUNT,

                InputVertices = _vertices,
                OutputVertices = _currentVertices
            };
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
            Mesh newMesh = _meshFilter.mesh;
            _currentVertices = _segmentMoveJob.OutputVertices;
            newMesh.SetVertexBufferData(_currentVertices, 0, 0,
                VERTEX_COUNT, 0,
                MESH_UPDATE_FLAGS
            );

            newMesh.RecalculateNormals();
            _meshFilter.mesh = newMesh;

            for (int i = 0; i < _vertices.Length; i++)
            {
                _vertices[i] = _currentVertices[i];
            }
            _moveCompleteAction.Invoke(_currentMove.ToIndex);
            _wasMoveCompleted = false;
        }
        else if (_wasJobScheduled)
        {
            _segmentMoveJobHandle.Complete();
            Mesh newMesh = _meshFilter.mesh;
            _currentVertices = _segmentMoveJob.OutputVertices;
            newMesh.SetVertexBufferData(_currentVertices, 0, 0,
                VERTEX_COUNT, 0,
                MESH_UPDATE_FLAGS
            );

            newMesh.RecalculateNormals();
            _meshFilter.mesh = newMesh;
            
            _wasJobScheduled = false;
        }
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