using System.Collections;

using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Assertions;

using Orazum.Constants;
using Orazum.Collections;
using Orazum.Meshing;

public abstract class FigureGenerator : MonoBehaviour
{
    [SerializeField]
    private FigureParamsSO _figureParams;

    protected const float SegmentPointHeight = 1;
    protected const MeshUpdateFlags GenerationMeshUpdateFlags = MeshUpdateFlags.Default;

    protected int2 _dims;
    protected Figure _figure;
    protected Array2D<FigureSegment> _segments;
    protected Array2D<FigureSegmentPoint> _segmentPoints;


    private FigureTransitionsGenerator _transitionsGenerator;
    private void Awake()
    {
        StartCoroutine(FigureGenerationSequence());
    }
    private IEnumerator FigureGenerationSequence()
    {
        bool isFound = TryGetComponent(out _transitionsGenerator);
        Assert.IsTrue(isFound);

        InitializeParameters(_figureParams.GenParams);
        StartMeshGeneration();
        GenerateFigureGameObject();
        yield return null;
        CompleteGeneration(_figureParams);
        _transitionsGenerator.StartGeneration(_quadStripsCollection, _figureMeshGenJobHandle);
        yield return null;
        _transitionsGenerator.FinishGeneration(_figure);
        FigureDelegatesContainer.EventFigureGenerationCompleted?.Invoke(_figure);
    }

    protected abstract void InitializeParameters(FigureGenParamsSO figureGenParams);

    protected JobHandle _figureMeshGenJobHandle;
    protected NativeArray<VertexData> _figureVertices;
    protected NativeArray<short> _figureIndices;

    protected JobHandle _segmentPointsMeshGenJobHandle;
    protected NativeArray<float3> _pointsRenderVertices;
    protected NativeArray<short> _pointsRenderIndices;
    protected NativeArray<float3> _pointsColliderVertices;
    protected NativeArray<short> _pointsColliderIndices;

    protected QuadStripsBuffer _quadStripsCollection;
    protected abstract void StartMeshGeneration();

    private void GenerateFigureGameObject()
    {
        GameObject figureGb = GenerateFigureGb();
        _figure = figureGb.GetComponent<Figure>();
        Assert.IsNotNull(_figure);
        figureGb.layer = Layers.FigureLayer;

        GameObject segmentPointsParentGb = new GameObject(FigureName + "SegmentPoints");
        Transform segmentPointsParent = segmentPointsParentGb.transform;
        segmentPointsParent.parent = figureGb.transform;
        segmentPointsParent.SetSiblingIndex(0);

        GameObject segmentsParentGb = new GameObject(FigureName + "Segments");
        Transform segmentsParent = segmentsParentGb.transform;
        segmentsParent.parent = figureGb.transform;
        segmentsParent.SetSiblingIndex(1);

        _segments = new Array2D<FigureSegment>(_dims);
        _segmentPoints = new Array2D<FigureSegmentPoint>(_dims);

        for (int col = 0; col < _dims.x; col++)
        {
            for (int row = 0; row < _dims.y; row++)
            {
                int2 index = new int2(col, row);

                GameObject segmentGb = new GameObject(FigureName + "Segment", typeof(MeshFilter), typeof(MeshRenderer));
                segmentGb.transform.parent = segmentsParent;
                FigureSegment segment = AddSegmentComponent(segmentGb);
                Assert.IsNotNull(segment);
                segment.PrepareRenderer(_figureParams.GenParams.DefaultMaterial, _figureParams.GenParams.HighlightMaterial);
                _segments[index] = segment;

                GameObject segmentPointGb = new GameObject(FigureName + "SegmentPoint", typeof(MeshFilter), typeof(MeshRenderer));
                segmentPointGb.layer = Layers.SegmentPointsLayer;
                segmentPointGb.transform.parent = segmentPointsParent;
                FigureSegmentPoint segmentPoint = AddSegmentPointComponent(segmentPointGb);
                Assert.IsNotNull(segmentPoint);
                segmentPoint.Segment = segment;
                segmentPoint.AssignIndex(index);
                _segmentPoints[index] = segmentPoint;
            }
        }
    }
    protected abstract string FigureName { get; }
    protected abstract GameObject GenerateFigureGb();
    protected abstract FigureSegment AddSegmentComponent(GameObject segmentGb);
    protected abstract FigureSegmentPoint AddSegmentPointComponent(GameObject segmentPointGb);

    protected abstract void CompleteGeneration(FigureParamsSO figureParams);

    protected void UpdateSegment(FigureSegment segment, in MeshBuffersIndexers indexers, int puzzleIndex, int2 meshBuffersMaxCount)
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

        segment.AssignPuzzleIndex(puzzleIndex);
        segment.PrepareMover(_figureVertices[indexers.Start.x].uv, meshBuffersMaxCount);
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