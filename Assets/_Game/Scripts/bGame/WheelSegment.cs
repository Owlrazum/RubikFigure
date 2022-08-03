using System;
using System.Collections;

using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter))]
public class WheelSegment : MonoBehaviour
{
    private const float CLOCK_MOVE_BUFFER_LERP_VALUE = 0.49f;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    public MeshFilter MeshContainer { get { return _meshFilter; } }
    private int _segmentIndex;

    private NativeArray<VertexData> _vertices;
    private bool _needsDispose;

    private SegmentMoveJob _segmentMoveJob;
    private JobHandle _segmentMoveJobHandle;

    private NativeArray<VertexData> _currentVertices;
    private SegmentPoint _currentTarget;
    private float _currentSpeed;
    private SegmentMoveType _currentMove;

    private void Awake()
    { 
        TryGetComponent(out _meshFilter);
        TryGetComponent(out _meshRenderer);
    }

    public void Initialize(Material materialArg, NativeArray<VertexData> verticesArg, int segmentIndexArg)
    {
        _meshRenderer.sharedMaterial = materialArg;

        _vertices = verticesArg;
        _needsDispose = true;

        _currentVertices = 
            new NativeArray<VertexData>(_vertices.Length, Allocator.Persistent);

        _segmentIndex = segmentIndexArg;
    }

    public void StartSchedulingMoveJobs(
        SegmentPoint target, 
        float speed, 
        SegmentMoveType moveType, 
        Action<int> OnCompleteJobSchedule)
    {
        _currentTarget = target;
        _currentSpeed = speed;
        _currentMove = moveType;

        // for (int corner = 0; corner < 1; corner++)
        // { 
        //     int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner);
        //     float3 cornerPos = target.GetCornerPosition(corner);

        //     Debug.DrawLine(_vertices[meshCorner.x].position, cornerPos, Color.red, 100, false);
        //     Debug.DrawLine(_vertices[meshCorner.y].position, cornerPos, Color.red, 100, false);
        //     Debug.DrawLine(_vertices[meshCorner.z].position, cornerPos, Color.red, 100, false);
        // }
        StartCoroutine(MoveSequence(OnCompleteJobSchedule));
    }

    private IEnumerator MoveSequence(Action<int> OnCompleteJobSchedule)
    {
        float lerpParam = 0;
        while (lerpParam < 1)
        {
            lerpParam += _currentSpeed * Time.deltaTime;
            _segmentMoveJob = new SegmentMoveJob()
            {
                P_ClockMoveBufferLerpValue = CLOCK_MOVE_BUFFER_LERP_VALUE,
                P_LerpParam = MathUtilities.EaseInOut(lerpParam),
                P_SegmentMoveType = _currentMove,
                P_SegmentPoint = _currentTarget,
                P_VertexCountInOneSegment = WheelGenerator.VERTEX_COUNT_ONE_SEGMENT,

                InputVertices = _vertices,
                OutputVertices = _currentVertices
            };
            _segmentMoveJobHandle = _segmentMoveJob.Schedule();
            yield return null;
        }

        OnCompleteJobSchedule?.Invoke(_segmentIndex);
        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertices[i] = _currentVertices[i];
        }
    }

    public void CompleteMoveJob()
    {
        _segmentMoveJobHandle.Complete();
        Mesh newMesh = _meshFilter.mesh;
        _currentVertices = _segmentMoveJob.OutputVertices;
        newMesh.SetVertexBufferData(_currentVertices, 0, 0, 
            WheelGenerator.VERTEX_COUNT_ONE_SEGMENT, 0, 
            WheelGenerator.MESH_UPDATE_FLAGS
        );

        newMesh.RecalculateNormals();
        _meshFilter.mesh = newMesh;
    }

    private void OnDestroy()
    {
        if (_currentVertices.IsCreated)
        {
            _currentVertices.Dispose();
        }

        if (_vertices.IsCreated)
        {
            _vertices.Dispose();
        }
    }
}
