using System.Collections;

using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;

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

    protected QuadStripsCollection _quadStripsCollection;

    public FigureStatesController FinishGeneration(FigureParamsSO figureParams)
    {
        FigureStatesController figureStatesController = CompleteGeneration(figureParams);
        StartCoroutine(ShuffleTransitionsGeneration());
        return figureStatesController;
    }
    protected abstract FigureStatesController CompleteGeneration(FigureParamsSO figureParams);

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

    private IEnumerator ShuffleTransitionsGeneration()
    {
        yield return null;
    }

    protected void UpdateSegment(FigureSegment segment, in MeshBuffersIndexers indexers, int2 meshResPuzzleIndex)
    {
        Mesh mesh = segment.MeshContainer.mesh;
        mesh.MarkDynamic();

        mesh.SetVertexBufferParams(indexers.Count.x, VertexData.VertexBufferMemoryLayout);
        mesh.SetIndexBufferParams(indexers.Count.y, IndexFormat.UInt16);

        mesh.SetVertexBufferData(_figureVertices, indexers.Start.x, 0, indexers.Count.x, 0, GenerationMeshUpdateFlags);
        mesh.SetIndexBufferData(_figureIndices, indexers.Start.y, 0, indexers.Count.y, GenerationMeshUpdateFlags);

        mesh.subMeshCount = 1;
        SubMeshDescriptor subMesh = new SubMeshDescriptor(
            indexStart: 0,
            indexCount: indexers.Count.y
        );
        mesh.SetSubMesh(0, subMesh);

        mesh.RecalculateBounds();

        segment.Initialize(_figureVertices[indexers.Start.x].uv, meshResPuzzleIndex.x, meshResPuzzleIndex.y);
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
        _quadStripsCollection.DisposeIfNeeded();

        CollectionUtilities.DisposeIfNeeded(_figureVertices);
        CollectionUtilities.DisposeIfNeeded(_figureIndices);

        CollectionUtilities.DisposeIfNeeded(_pointsRenderVertices);
        CollectionUtilities.DisposeIfNeeded(_pointsRenderIndices);

        CollectionUtilities.DisposeIfNeeded(_pointsColliderVertices);
        CollectionUtilities.DisposeIfNeeded(_pointsColliderIndices);
    }
}