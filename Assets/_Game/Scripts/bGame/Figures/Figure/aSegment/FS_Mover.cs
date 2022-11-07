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
public class FS_Mover : MonoBehaviour
{
    private const float ClockMoveBufferLerpValue = 0.4f;
    private const MeshUpdateFlags MoveMeshUpdateFlags = MeshUpdateFlags.Default;

    private MeshFilter _meshFilter;
    public MeshFilter MeshContainer { get { return _meshFilter; } }

    protected JobHandle _moveJobHandle;

    protected float _currentLerpSpeed;

    protected Action _moveCompleteAction;

    private NativeArray<VertexData> _vertices;
    private NativeArray<short> _indices;
    private MeshBuffersIndexersForJob _indexersForJob;

    private QST_Animator _animator_QST;

    private bool _wasJobScheduled;
    private bool _wasMoveCompleted;

    private bool _shouldDispose;
    private QS_Transition _toDispose;

    private float2 _uv;

    public void Initialize(float2 uv, int2 meshBuffersMaxCount)
    {
        _vertices = new NativeArray<VertexData>(meshBuffersMaxCount.x, Allocator.Persistent);
        _indices = new NativeArray<short>(meshBuffersMaxCount.y, Allocator.Persistent);
        _indexersForJob = new MeshBuffersIndexersForJob(new MeshBuffersIndexers());

        _uv = uv;
        float3x2 normalUV = new float3x2(math.up(), new float3(uv, 0));
        _animator_QST = new QST_Animator( _vertices, _indices, normalUV);
    }

    protected virtual void Awake()
    {
        TryGetComponent(out _meshFilter);
    }

    protected virtual void OnDestroy()
    {
        if (!_moveJobHandle.IsCompleted)
        {
            _moveJobHandle.Complete();
        }

        if (_shouldDispose)
        {
            _toDispose.DisposeConcatenationIfNeeded();
        }

        CollectionUtilities.DisposeIfNeeded(_vertices);
        CollectionUtilities.DisposeIfNeeded(_indices);
        _indexersForJob.DisposeIfNeeded();
    }

    public virtual void StartMove(
        FigureMoveOnSegment move,
        Action onMoveCompleted)
    {
        _moveCompleteAction = onMoveCompleted;
        _currentLerpSpeed = move.LerpSpeed;

        switch (move)
        { 
            case FMSC_Transition fmsct:
                _shouldDispose = fmsct.ShouldDisposeTransition;
                if (_shouldDispose)
                {
                    _toDispose = fmsct.Transition;
                }
                StartCoroutine(MoveWithTransitionSequence(fmsct.Transition));
                break;
            case FMS_Transition fmst:
                _shouldDispose = fmst.ShouldDisposeTransition;
                if (_shouldDispose)
                {
                    _toDispose = fmst.Transition;
                }
                StartCoroutine(MoveWithTransitionSequence(fmst.Transition));
                break;
            default:
                throw new ArgumentException("Unknown type of move");
        }
    }

     private IEnumerator MoveWithTransitionSequence(QS_Transition transition)
    {
        float lerpParam = 0;
        Assert.IsTrue(transition.IsCreated);
        _animator_QST.AssignTransition(transition);
        FS_MoveJob moveJob = new FS_MoveJob()
        {
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
            _moveJobHandle = moveJob.Schedule(_moveJobHandle);
            _wasJobScheduled = true;
            yield return null;
        }

        _wasMoveCompleted = true;
        
        if (_shouldDispose)
        {
            _toDispose.DisposeConcatenation();
        }
    }

    private void LateUpdate()
    {
        if (_wasJobScheduled)
        { 
            _moveJobHandle.Complete();
            AssignMeshBuffers();
            _indexersForJob.Reset();
            
            _wasJobScheduled = false;
        }

        if (_wasMoveCompleted)
        { 
            _wasMoveCompleted = false;
            _moveCompleteAction?.Invoke();
        }
    }

    protected void AssignMeshBuffers()
    { 
        Mesh mesh = MeshContainer.mesh;
        mesh.MarkDynamic();

        MeshBuffersIndexers buffersIndexers = _indexersForJob.GetIndexersOutsideJob();
        mesh.SetVertexBufferParams(buffersIndexers.Count.x, VertexData.VertexBufferMemoryLayout);
        mesh.SetIndexBufferParams(buffersIndexers.Count.y, IndexFormat.UInt16);

        mesh.SetVertexBufferData(_vertices, buffersIndexers.Start.x, 0, buffersIndexers.Count.x, 0, MoveMeshUpdateFlags);
        mesh.SetIndexBufferData(_indices, buffersIndexers.Start.y, 0, buffersIndexers.Count.y, MoveMeshUpdateFlags);

        mesh.subMeshCount = 1;
        SubMeshDescriptor subMesh = new SubMeshDescriptor(
            indexStart: 0,
            indexCount: buffersIndexers.Count.y
        );
        mesh.SetSubMesh(0, subMesh);

        // mesh.RecalculateNormals();
        mesh.RecalculateBounds();
    }
}