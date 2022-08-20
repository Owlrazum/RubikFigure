using System;

using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

using Orazum.Collections;

[RequireComponent(typeof(MeshFilter))]
public abstract class FigureSegmentMover : MonoBehaviour
{ 
    public const MeshUpdateFlags MESH_UPDATE_FLAGS = MeshUpdateFlags.Default;

    private MeshFilter _meshFilter;
    public MeshFilter MeshContainer { get { return _meshFilter; } }

    protected NativeArray<VertexData> _vertices;
    protected NativeArray<VertexData> _currentVertices;

    protected JobHandle _segmentMoveJobHandle;

    protected float _currentLerpSpeed;
    protected int2 _currentToIndex;

    protected Action _moveCompleteAction;

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

        CollectionUtilities.DisposeIfNeeded(_currentVertices);
        CollectionUtilities.DisposeIfNeeded(_vertices);
    }

    public virtual void Initialize(NativeArray<VertexData> verticesArg)
    {
        _vertices = verticesArg;

        _currentVertices =
            new NativeArray<VertexData>(_vertices.Length, Allocator.Persistent);
    }

    public virtual void StartMove(
        FigureSegmentMove move,
        float _lerpSpeed,
        Action OnMoveToDestinationCompleted)
    {
        _currentToIndex = move.ToIndex;
        _currentLerpSpeed = _lerpSpeed;
        _moveCompleteAction = OnMoveToDestinationCompleted;
    }

    protected void AssignVertices(NativeArray<VertexData> toAssign, int vertexCount)
    {
        Mesh newMesh = _meshFilter.mesh;
        newMesh.SetVertexBufferData(toAssign, 0, 0,
            vertexCount, 0,
            MESH_UPDATE_FLAGS
        );

        newMesh.RecalculateNormals();
        _meshFilter.mesh = newMesh;
    }

    public void Appear()
    {
        gameObject.SetActive(true);
    }
}