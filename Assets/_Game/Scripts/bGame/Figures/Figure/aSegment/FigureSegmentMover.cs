using System;
using System.Collections;

using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

using Orazum.Meshing;
using Orazum.Collections;
using static Orazum.Math.EasingUtilities;

[RequireComponent(typeof(MeshFilter))]
public abstract class FigureSegmentMover : MonoBehaviour
{
    protected int MeshResolution { get; private set; }
    protected abstract int MaxVertexCount { get; }
    protected abstract int MaxIndexCount { get; }

    private const float ClockMoveBufferLerpValue = 0.4f;
    private const MeshUpdateFlags MoveMeshUpdateFlags = MeshUpdateFlags.Default;

    private MeshFilter _meshFilter;
    public MeshFilter MeshContainer { get { return _meshFilter; } }

    protected JobHandle _segmentMoveJobHandle;

    protected float _currentLerpSpeed;
    protected int2 _currentToIndex;

    protected Action _moveCompleteAction;

    private NativeArray<VertexData> _vertices;
    private NativeArray<short> _indices;
    private MeshBuffersIndexersForJob _indexersForJob;

    private QSTransition _quadStripTransition;
    private float3x2 _normalUV;

    private bool _wasJobScheduled;
    private bool _wasMoveCompleted;

    public void Initialize(float2 uv, int meshResolution)
    {
        MeshResolution = meshResolution;
        _vertices = new NativeArray<VertexData>(MaxVertexCount, Allocator.Persistent);
        _indices = new NativeArray<short>(MaxIndexCount, Allocator.Persistent);
        _indexersForJob = new MeshBuffersIndexersForJob(new MeshBuffersIndexers());
        
        _quadStripTransition = new QSTransition(ref _vertices, ref _indices);
        _normalUV = new float3x2(math.up(), new float3(uv, 0));
    }

    protected virtual void Awake()
    {
        TryGetComponent(out _meshFilter);
    }

    protected virtual void OnDestroy()
    {
        if (!_segmentMoveJobHandle.IsCompleted)
        {
            _segmentMoveJobHandle.Complete();
        }

        CollectionUtilities.DisposeIfNeeded(_vertices);
        CollectionUtilities.DisposeIfNeeded(_indices);
        _indexersForJob.DisposeIfNeeded();
    }

    public virtual void StartMove(
        FigureSegmentMove move,
        Action OnMoveToDestinationCompleted)
    {
        _currentToIndex = move.ToIndex;
        _currentLerpSpeed = move.LerpSpeed;
        _moveCompleteAction = OnMoveToDestinationCompleted;

        if (move is FigureVerticesMove verticesMove)
        {
            Debug.Log("Starting vertices move");
            StartCoroutine(MoveSequence(verticesMove));
        }
    }

     private IEnumerator MoveSequence(FigureVerticesMove verticesMove)
    {
        float lerpParam = 0;
        Assert.IsTrue(verticesMove.TransSegments.IsCreated);
        _quadStripTransition.AssignTransitionData(verticesMove.TransSegments, _normalUV);
        FigureSegmentMoveJob moveJob = new FigureSegmentMoveJob()
        {
            InputQuadStripTransition = _quadStripTransition,
            OutputIndexers = _indexersForJob
        };

        while (lerpParam < 1)
        {
            lerpParam += _currentLerpSpeed * Time.deltaTime;
            if (lerpParam > 1)
            {
                lerpParam = 1;
            }

            moveJob.P_LerpParam = EaseInOut(lerpParam);
            _segmentMoveJobHandle = moveJob.Schedule(_segmentMoveJobHandle);
            _wasJobScheduled = true;
            yield return null;
        }

        _wasMoveCompleted = true;
    }

    private void LateUpdate()
    {
        if (_wasMoveCompleted)
        {
            _indexersForJob.Reset();
            _moveCompleteAction?.Invoke();
            _wasMoveCompleted = false;
        }
        else if (_wasJobScheduled)
        {
            _segmentMoveJobHandle.Complete();
            AssignMeshBuffers(_vertices, _indices, _indexersForJob.GetIndexersOutsideJob());
            _indexersForJob.Reset();
            
            _wasJobScheduled = false;
        }
    }

    protected void AssignMeshBuffers(
        NativeArray<VertexData> vertices, 
        NativeArray<short> indices,
        in MeshBuffersIndexers buffersData
    )
    { 
        Mesh mesh = MeshContainer.mesh;
        mesh.MarkDynamic();

        mesh.SetVertexBufferParams(buffersData.Count.x, VertexData.VertexBufferMemoryLayout);
        mesh.SetIndexBufferParams(buffersData.Count.y, IndexFormat.UInt16);

        mesh.SetVertexBufferData(vertices, buffersData.Start.x, 0, buffersData.Count.x, 0, MoveMeshUpdateFlags);
        mesh.SetIndexBufferData(indices, buffersData.Start.y, 0, buffersData.Count.y, MoveMeshUpdateFlags);

        mesh.subMeshCount = 1;
        SubMeshDescriptor subMesh = new SubMeshDescriptor(
            indexStart: 0,
            indexCount: buffersData.Count.y
        );
        mesh.SetSubMesh(0, subMesh);

        // mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }

    public void Appear()
    {
        gameObject.SetActive(true);
    }
}