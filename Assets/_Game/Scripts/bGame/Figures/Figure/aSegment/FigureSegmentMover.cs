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

    private QST_Animator _animator_QST;

    private bool _wasJobScheduled;
    private bool _wasMoveCompleted;

    private bool _shouldDispose;
    private QS_Transition toDispose; 

    public void Initialize(float2 uv, int meshResolution)
    {
        MeshResolution = meshResolution;
        _vertices = new NativeArray<VertexData>(MaxVertexCount, Allocator.Persistent);
        _indices = new NativeArray<short>(MaxIndexCount, Allocator.Persistent);
        _indexersForJob = new MeshBuffersIndexersForJob(new MeshBuffersIndexers());
        
        float3x2 normalUV = new float3x2(math.up(), new float3(uv, 0));
        _animator_QST = new QST_Animator( _vertices, _indices, normalUV);
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

        if (_shouldDispose)
        {
            toDispose.DisposeIfNeededConcatenation();
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
            if (verticesMove.ShouldDisposeTransition)
            {
                _shouldDispose = true;
                toDispose = verticesMove.Transition;
            }
            Debug.Log("Starting vertices move");
            StartCoroutine(MoveSequence(verticesMove));
        }
        else
        {
            Debug.LogError("Unknown type of move");
        }
    }

     private IEnumerator MoveSequence(FigureVerticesMove verticesMove)
    {
        float lerpParam = 0;
        Assert.IsTrue(verticesMove.Transition.IsCreated);
        _animator_QST.AssignTransition(verticesMove.Transition);
        print(verticesMove.Transition);
        FigureSegmentMoveJob moveJob = new FigureSegmentMoveJob()
        {
            P_ShouldReorientVertices = verticesMove.ShouldReorientVertices,
            InputQuadStripTransition = _animator_QST,
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
        
        if (_shouldDispose)
        {
            toDispose.DisposeConcatenation();
        }
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
        in MeshBuffersIndexers buffersIndexers
    )
    { 
        Mesh mesh = MeshContainer.mesh;
        mesh.MarkDynamic();

        mesh.SetVertexBufferParams(buffersIndexers.Count.x, VertexData.VertexBufferMemoryLayout);
        mesh.SetIndexBufferParams(buffersIndexers.Count.y, IndexFormat.UInt16);

        mesh.SetVertexBufferData(vertices, buffersIndexers.Start.x, 0, buffersIndexers.Count.x, 0, MoveMeshUpdateFlags);
        mesh.SetIndexBufferData(indices, buffersIndexers.Start.y, 0, buffersIndexers.Count.y, MoveMeshUpdateFlags);

        mesh.subMeshCount = 1;
        SubMeshDescriptor subMesh = new SubMeshDescriptor(
            indexStart: 0,
            indexCount: buffersIndexers.Count.y
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