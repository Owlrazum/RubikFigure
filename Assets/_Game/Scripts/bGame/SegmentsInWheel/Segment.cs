using System;
using System.Collections;

using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Utilities;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Assertions;



/// <summary>
/// Segment of the Wheel
/// </summary>
[RequireComponent(typeof(MeshFilter))]
public class Segment : MonoBehaviour
{
    public const int VERTEX_COUNT = 24;
    public const int INDEX_COUNT = 36;
    public const MeshUpdateFlags MESH_UPDATE_FLAGS = MeshUpdateFlags.Default;

    private const float CLOCK_MOVE_BUFFER_LERP_VALUE = 0.4f;

    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    public MeshFilter MeshContainer { get { return _meshFilter; } }
    private int2 _segmentIndex;
    private int _colorIndex;
    public int ColorIndex{ get { return _colorIndex; } }

    private NativeArray<VertexData> _vertices;
    private bool _needsDispose;

    private SegmentMoveJob _segmentMoveJob;
    private JobHandle _segmentMoveJobHandle;

    private NativeArray<VertexData> _currentVertices;
    private float _currentSpeed;
    private SegmentMove _currentMove;

    private bool wasJobCompleted;

    private void Awake()
    {
        TryGetComponent(out _meshFilter);
        TryGetComponent(out _meshRenderer);
    }

    public void Initialize(
        Material materialArg, 
        NativeArray<VertexData> verticesArg, 
        int2 segmentIndexArg, 
        int colorIndexArg)
    {
        _meshRenderer.sharedMaterial = materialArg;

        _vertices = verticesArg;
        _needsDispose = true;

        _currentVertices =
            new NativeArray<VertexData>(_vertices.Length, Allocator.Persistent);

        _segmentIndex = segmentIndexArg;
        _colorIndex = colorIndexArg;

        wasJobCompleted = true;
    }

    public void StartSchedulingMoveJobs(
        SegmentMove move,
        float speed,
        Action<SegmentMove> OnSegmentsCompletedMove)
    {
        _currentMove = move;
        _currentSpeed = speed;

        StartCoroutine(MoveSequence(OnSegmentsCompletedMove));
    }

    private IEnumerator MoveSequence(Action<SegmentMove> OnSegmentsCompletedMove)
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
                P_LerpParam = MathUtilities.EaseInOut(lerpParam),
                P_SegmentMoveType = _currentMove.MoveType,
                P_SegmentPoint = _currentMove.GetTargetCornerPositions(),
                P_VertexCountInOneSegment = VERTEX_COUNT,

                InputVertices = _vertices,
                OutputVertices = _currentVertices
            };
            
            // try
            // { 
                Assert.IsTrue(wasJobCompleted);
            // } catch (AssertionException assertion)
            // {
            //     Debug.Log("The lerp param is " + lerpParam + " " +_currentSpeed * Time.deltaTime);
            //     Debug.Break();
            // }

            wasJobCompleted = false;
            _segmentMoveJobHandle = _segmentMoveJob.Schedule(_segmentMoveJobHandle);
            yield return null;
        }

        OnSegmentsCompletedMove.Invoke(_currentMove);
    }

    public void CompleteMoveJob()
    {
        wasJobCompleted = true;
        _segmentMoveJobHandle.Complete();
        Mesh newMesh = _meshFilter.mesh;
        _currentVertices = _segmentMoveJob.OutputVertices;
        newMesh.SetVertexBufferData(_currentVertices, 0, 0,
            VERTEX_COUNT, 0,
            MESH_UPDATE_FLAGS
        );

        newMesh.RecalculateNormals();
        _meshFilter.mesh = newMesh;
    }

    public void OnMoveComplete()
    {
        for (int i = 0; i < _vertices.Length; i++)
        {
            _vertices[i] = _currentVertices[i];
        }
    }

    public void Dissappear()
    {
        Destroy(gameObject);
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
