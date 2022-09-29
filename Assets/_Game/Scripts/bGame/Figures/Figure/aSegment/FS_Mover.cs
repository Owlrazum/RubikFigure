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
public class FigureSegmentMover : MonoBehaviour
{
    private const float ClockMoveBufferLerpValue = 0.4f;
    private const MeshUpdateFlags MoveMeshUpdateFlags = MeshUpdateFlags.Default;

    private MeshFilter _meshFilter;
    public MeshFilter MeshContainer { get { return _meshFilter; } }

    protected JobHandle _segmentMoveJobHandle;

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

    public void Initialize(float2 uv, int2 meshBuffersMaxCount)
    {
        _vertices = new NativeArray<VertexData>(meshBuffersMaxCount.x, Allocator.Persistent);
        _indices = new NativeArray<short>(meshBuffersMaxCount.y, Allocator.Persistent);
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
            _toDispose.DisposeConcatenationIfNeeded();
        }

        CollectionUtilities.DisposeIfNeeded(_vertices);
        CollectionUtilities.DisposeIfNeeded(_indices);
        _indexersForJob.DisposeIfNeeded();
    }

    public virtual void StartMove(
        FS_Movement move,
        Action OnMoveToDestinationCompleted)
    {
        _moveCompleteAction = OnMoveToDestinationCompleted;
        _currentLerpSpeed = move.LerpSpeed;

        switch (move)
        { 
            case FSMC_Transition fsmct:
                _shouldDispose = fsmct.ShouldDisposeTransition;
                if (_shouldDispose)
                {
                    _toDispose = fsmct.Transition;
                }
                StartCoroutine(MoveSequence(fsmct.Transition));
                break;
            case FSM_Transition fsmt:
                _shouldDispose = fsmt.ShouldDisposeTransition;
                if (_shouldDispose)
                {
                    _toDispose = fsmt.Transition;
                }
                StartCoroutine(MoveSequence(fsmt.Transition));
                break;
            default:
                throw new ArgumentException("Unknown type of move");
        }
    }

     private IEnumerator MoveSequence(QS_Transition transition)
    {
        float lerpParam = 0;
        Assert.IsTrue(transition.IsCreated);
        _animator_QST.AssignTransition(transition);
        FigureSegmentMoveJob moveJob = new FigureSegmentMoveJob()
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
            _segmentMoveJobHandle = moveJob.Schedule(_segmentMoveJobHandle);
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
}