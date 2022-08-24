using System;

using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

using Orazum.Meshing;

[RequireComponent(typeof(MeshFilter))]
public abstract class FigureSegmentMover : MonoBehaviour
{ 
    public const MeshUpdateFlags MoveMeshUpdateFlags = MeshUpdateFlags.Default;

    private MeshFilter _meshFilter;
    public MeshFilter MeshContainer { get { return _meshFilter; } }

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
    }

    public abstract void Initialize(NativeArray<VertexData> verticesArg);

    public virtual void StartMove(
        FigureSegmentMove move,
        Action OnMoveToDestinationCompleted)
    {
        _currentToIndex = move.ToIndex;
        _currentLerpSpeed = move.LerpSpeed;
        _moveCompleteAction = OnMoveToDestinationCompleted;
    }

    protected void AssignVertices(NativeArray<VertexData> toAssign, int vertexCount)
    {
        Mesh newMesh = _meshFilter.mesh;
        newMesh.SetVertexBufferData(toAssign, 0, 0,
            vertexCount, 0,
            MoveMeshUpdateFlags
        );

        newMesh.RecalculateNormals();
        _meshFilter.mesh = newMesh;
    }

    protected void AssignMeshBuffers(
        NativeArray<VertexData> vertices, 
        NativeArray<short> indices,
        in MeshBuffersData buffersData
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

        mesh.RecalculateBounds();
    }

    public void Appear()
    {
        gameObject.SetActive(true);
    }
}