using System.Collections;

using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Assertions;

using Orazum.Collections;
using Orazum.Meshing;

public abstract class FigureGeneratorGameObject : MonoBehaviour
{
    [SerializeField]
    private FigureParamsSO _figureParams;

    private Figure _figure;
    private FigureGeneratorTransitions _transitionsGenerator;

    private void Awake()
    {
        bool isFound = TryGetComponent(out _transitionsGenerator);
        Assert.IsTrue(isFound);
        StartFigureGeneration();
        FigureDelegatesContainer.GetFigure += GetFigure;
        FigureDelegatesContainer.FinishMeshGeneration += FinishFigureGeneration;
        FigureDelegatesContainer.FinishShuffleTransitionsGeneration += FinishShuffleTransitionsGeneration;
    }

    private void Unsubscribe()
    {
        FigureDelegatesContainer.GetFigure -= GetFigure;
        FigureDelegatesContainer.FinishMeshGeneration -= FinishFigureGeneration;
        FigureDelegatesContainer.FinishShuffleTransitionsGeneration -= FinishShuffleTransitionsGeneration;
    }

    private void StartFigureGeneration()
    {
        InitializeParameters(_figureParams.FigureGenParamsSO);
        StartMeshGeneration();
        _figure = GenerateFigureGameObject();
    }

    protected abstract void StartMeshGeneration();
    protected abstract Figure GenerateFigureGameObject();

    private void FinishFigureGeneration()
    {
        CompleteGeneration(_figureParams);
        _transitionsGenerator.StartGeneration(_quadStripsCollection, _figureMeshGenJobHandle);
    }

    protected abstract void CompleteGeneration(FigureParamsSO figureParams);

    private void FinishShuffleTransitionsGeneration()
    {
        _transitionsGenerator.FinishGeneration(_figure);
    }

    private Figure GetFigure()
    {
        return _figure;
    }

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

    protected QuadStripsBuffer _quadStripsCollection;

    protected virtual void InitializeParameters(FigureGenParamsSO figureGenParams)
    { 
        _segmentPointHeight = figureGenParams.Height;

        _segmentPrefab = figureGenParams.SegmentPrefab;
        _segmentPointPrefab = figureGenParams.SegmentPointPrefab;
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
        Unsubscribe();

        _quadStripsCollection.DisposeIfNeeded();

        CollectionUtilities.DisposeIfNeeded(_figureVertices);
        CollectionUtilities.DisposeIfNeeded(_figureIndices);

        CollectionUtilities.DisposeIfNeeded(_pointsRenderVertices);
        CollectionUtilities.DisposeIfNeeded(_pointsRenderIndices);

        CollectionUtilities.DisposeIfNeeded(_pointsColliderVertices);
        CollectionUtilities.DisposeIfNeeded(_pointsColliderIndices);
    }
}