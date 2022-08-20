using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

using Unity.Mathematics;
using Orazum.Collections;

public abstract class FigureGenerator : MonoBehaviour
{ 
    protected const MeshUpdateFlags MESH_UPDATE_FLAGS = MeshUpdateFlags.Default;

    protected float _segmentPointHeight;

    protected JobHandle _figureMeshGenJobHandle;
    protected NativeArray<VertexData> _figureVertices;
    protected NativeArray<short> _figureIndices;

    protected JobHandle _segmentPointsMeshGenJobHandle;
    protected NativeArray<float3> _segmentPointsVertices;
    protected NativeArray<short> _segmentPointsIndices;

    public abstract Figure FinishGeneration(FigureParamsSO figureParams);

    public virtual void StartGeneration(FigureGenParamsSO figureGenParams)
    {
        InitializeParameters(figureGenParams);
        StartMeshGeneration();
        GenerateFigureGameObject();
    }
    protected abstract void InitializeParameters(FigureGenParamsSO figureGenParams);
    protected abstract void StartMeshGeneration();
    protected abstract void GenerateFigureGameObject();

    protected virtual void UpdateSegment(FigureSegment segment, in MeshBuffersData data, int puzzleIndex)
    {
        Mesh mesh = segment.MeshContainer.mesh;
        mesh.MarkDynamic();

        mesh.SetVertexBufferParams(data.Count.x, VertexData.VertexBufferMemoryLayout);
        mesh.SetIndexBufferParams(data.Count.y, IndexFormat.UInt16);

        mesh.SetVertexBufferData(_figureVertices, data.Start.x, 0, data.Count.x, 0, MESH_UPDATE_FLAGS);
        mesh.SetIndexBufferData(_figureIndices, data.Start.y, 0, data.Count.y, MESH_UPDATE_FLAGS);

        mesh.subMeshCount = 1;
        SubMeshDescriptor subMesh = new SubMeshDescriptor(
            indexStart: 0,
            indexCount: data.Count.y
        );
        mesh.SetSubMesh(0, subMesh);

        mesh.RecalculateBounds();

        NativeArray<VertexData> segmentVertices = CollectionUtilities.GetSlice(
            _figureVertices, data.Start.x, data.Count.x);

        segment.Initialize(segmentVertices, puzzleIndex);
    }
    
    protected virtual Mesh CreateSegmentPointMesh(in MeshBuffersData data)
    {
        Mesh segmentPointMesh = new Mesh();
        segmentPointMesh.MarkDynamic();

        segmentPointMesh.SetVertexBufferParams(data.Count.x, VertexData.PositionBufferMemoryLayout);
        segmentPointMesh.SetIndexBufferParams(data.Count.y, IndexFormat.UInt16);

        segmentPointMesh.SetVertexBufferData(_segmentPointsVertices, data.Start.x, 0, data.Count.x, 0, MESH_UPDATE_FLAGS);
        segmentPointMesh.SetIndexBufferData(_segmentPointsIndices, data.Start.y, 0, data.Count.y, MESH_UPDATE_FLAGS);

        segmentPointMesh.subMeshCount = 1;
        SubMeshDescriptor subMesh = new SubMeshDescriptor(
            indexStart: 0,
            indexCount: data.Count.y
        );
        segmentPointMesh.SetSubMesh(0, subMesh);

        segmentPointMesh.RecalculateBounds();

        return segmentPointMesh;
    }
    protected virtual void OnDestroy()
    {
        CollectionUtilities.DisposeIfNeeded(_figureVertices);
        CollectionUtilities.DisposeIfNeeded(_figureIndices);

        CollectionUtilities.DisposeIfNeeded(_segmentPointsVertices);
        CollectionUtilities.DisposeIfNeeded(_segmentPointsIndices);
    }
}