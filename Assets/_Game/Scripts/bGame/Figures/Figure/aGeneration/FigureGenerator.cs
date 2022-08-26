using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

using Unity.Mathematics;
using Orazum.Collections;
using Orazum.Meshing;

public abstract class FigureGenerator : MonoBehaviour
{
    protected const MeshUpdateFlags GenerationMeshUpdateFlags = MeshUpdateFlags.Default;

    protected float _segmentPointHeight;

    protected GameObject _segmentPrefab;
    protected GameObject _segmentPointPrefab;

    protected JobHandle _figureMeshGenJobHandle;
    protected NativeArray<VertexData> _figureVertices;
    protected NativeArray<short> _figureIndices;

    protected JobHandle _segmentPointsMeshGenJobHandle;
    protected NativeArray<float3> _pointsRenderVertices;
    protected NativeArray<short> _pointsRenderIndices;
    protected NativeArray<float3> _pointsColliderVertices;
    protected NativeArray<short> _pointsColliderIndices;

    public abstract Figure FinishGeneration(FigureParamsSO figureParams);

    public virtual void StartGeneration(FigureGenParamsSO figureGenParams)
    {
        InitializeParameters(figureGenParams);
        StartMeshGeneration();
        GenerateFigureGameObject();
    }
    protected virtual void InitializeParameters(FigureGenParamsSO figureGenParams)
    { 
        _segmentPointHeight = figureGenParams.Height;

        _segmentPrefab = figureGenParams.SegmentPrefab;
        _segmentPointPrefab = figureGenParams.SegmentPointPrefab;
    }
    protected abstract void StartMeshGeneration();
    protected abstract void GenerateFigureGameObject();

    protected virtual void UpdateSegment(FigureSegment segment, in MeshBuffersIndexers data, int puzzleIndex)
    {
        Mesh mesh = segment.MeshContainer.mesh;
        mesh.MarkDynamic();

        mesh.SetVertexBufferParams(data.Count.x, VertexData.VertexBufferMemoryLayout);
        mesh.SetIndexBufferParams(data.Count.y, IndexFormat.UInt16);

        mesh.SetVertexBufferData(_figureVertices, data.Start.x, 0, data.Count.x, 0, GenerationMeshUpdateFlags);
        mesh.SetIndexBufferData(_figureIndices, data.Start.y, 0, data.Count.y, GenerationMeshUpdateFlags);

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
    
    protected Mesh CreateSegmentPointRenderMesh(in MeshBuffersIndexers buffersData)
    {
        Mesh segmentPointMesh = new Mesh();
        segmentPointMesh.MarkDynamic();

        segmentPointMesh.SetVertexBufferParams(buffersData.Count.x, VertexData.PositionBufferMemoryLayout);
        segmentPointMesh.SetIndexBufferParams(buffersData.Count.y, IndexFormat.UInt16);

        segmentPointMesh.SetVertexBufferData(_pointsRenderVertices, buffersData.Start.x, 0, buffersData.Count.x, 0, GenerationMeshUpdateFlags);
        segmentPointMesh.SetIndexBufferData(_pointsRenderIndices, buffersData.Start.y, 0, buffersData.Count.y, GenerationMeshUpdateFlags);

        segmentPointMesh.subMeshCount = 1;
        SubMeshDescriptor subMesh = new SubMeshDescriptor(
            indexStart: 0,
            indexCount: buffersData.Count.y
        );
        segmentPointMesh.SetSubMesh(0, subMesh);

        segmentPointMesh.RecalculateBounds();

        return segmentPointMesh;
    }

    protected Mesh CreateSegmentPointColliderMesh(in MeshBuffersIndexers buffersData)
    { 
        Mesh segmentPointMesh = new Mesh();
        segmentPointMesh.MarkDynamic();

        segmentPointMesh.SetVertexBufferParams(buffersData.Count.x, VertexData.PositionBufferMemoryLayout);
        segmentPointMesh.SetIndexBufferParams(buffersData.Count.y, IndexFormat.UInt16);

        segmentPointMesh.SetVertexBufferData(_pointsColliderVertices, buffersData.Start.x, 0, buffersData.Count.x, 0, GenerationMeshUpdateFlags);
        segmentPointMesh.SetIndexBufferData(_pointsColliderIndices, buffersData.Start.y, 0, buffersData.Count.y, GenerationMeshUpdateFlags);

        segmentPointMesh.subMeshCount = 1;
        SubMeshDescriptor subMesh = new SubMeshDescriptor(
            indexStart: 0,
            indexCount: buffersData.Count.y
        );
        segmentPointMesh.SetSubMesh(0, subMesh);

        segmentPointMesh.RecalculateBounds();

        return segmentPointMesh;
    }

    protected virtual void OnDestroy()
    {
        CollectionUtilities.DisposeIfNeeded(_figureVertices);
        CollectionUtilities.DisposeIfNeeded(_figureIndices);

        CollectionUtilities.DisposeIfNeeded(_pointsRenderVertices);
        CollectionUtilities.DisposeIfNeeded(_pointsRenderIndices);

        CollectionUtilities.DisposeIfNeeded(_pointsColliderVertices);
        CollectionUtilities.DisposeIfNeeded(_pointsColliderIndices);
    }
}