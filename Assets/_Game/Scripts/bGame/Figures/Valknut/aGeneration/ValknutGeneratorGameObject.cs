using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Unity.Mathematics;
using Orazum.Constants;
using Orazum.Meshing;
using Orazum.Collections;

public class ValknutGeneratorGameObject : FigureGeneratorGameObject
{
    #region Constants
    private const int SegmentsCount = Valknut.TrianglesCount * Valknut.PartsCount; // three outer and three inner;
    private const int SegmentQuadsCountTAS = 3;
    private const int SegmentQuadsCountOAS = 2;

    private const int SegmentVertexCountTAS = (SegmentQuadsCountTAS + 1) * 2;
    private const int SegmentVertexCountOAS = (SegmentQuadsCountOAS + 1) * 2;
    private const int SegmentIndexCountTAS = SegmentQuadsCountTAS * 6;
    private const int SegmentIndexCountOAS = SegmentQuadsCountOAS * 6;

    private const int SegmentsTotalVertexCount = (SegmentVertexCountTAS + SegmentVertexCountOAS) * 3;
    private const int SegmentsTotalIndexCount = (SegmentIndexCountTAS + SegmentIndexCountOAS) * 3;

    private const int PointRendererVertexCountTAS = SegmentVertexCountTAS * 2;
    private const int PointRendererVertexCountOAS = SegmentVertexCountOAS * 2;
    private const int PointRendererTotalVertexCount = SegmentsTotalVertexCount * 2;

    private const int PointRendererIndexCountTAS = (SegmentQuadsCountTAS * 4 + 2) * 6;
    private const int PointRendererIndexCountOAS = (SegmentQuadsCountOAS * 4 + 2) * 6;
    private const int PointRendererTotalIndexCount = (PointRendererIndexCountTAS + PointRendererIndexCountOAS) * 3;

    private const int CubeVertexCount = 8;
    private const int CubeIndexCount = 6 * 6;

    private const int PointColliderVertexCountTAS = SegmentQuadsCountTAS * CubeVertexCount;
    private const int PointColliderVertexCountOAS = SegmentQuadsCountOAS * CubeVertexCount;
    private const int PointColliderTotalVertexCount = (PointColliderVertexCountTAS + PointColliderVertexCountOAS) * 3;

    private const int PointColliderIndexCountTAS = SegmentQuadsCountTAS * CubeIndexCount;
    private const int PointColliderIndexCountOAS = SegmentQuadsCountOAS * CubeIndexCount;
    private const int PointColliderTotalIndexCount = (PointColliderIndexCountTAS + PointColliderIndexCountOAS) * 3;
    #endregion

    [SerializeField]
    private FigureParamsSO _figureParams;

    private float _innerTriangleRadius;
    private float _width;
    private float _gapSize;

    private Valknut _valknut;

    private int2 _dims;

    private Array2D<ValknutSegment> _segments;
    private Array2D<FigureSegmentPoint> _segmentPoints;

    protected override void InitializeParameters(FigureGenParamsSO figureGenParams)
    {
        base.InitializeParameters(figureGenParams);

        ValknutGenParamsSO generationParams = figureGenParams as ValknutGenParamsSO;

        _innerTriangleRadius = generationParams.InnerTriangleRadius;
        _width = generationParams.Width;
        _gapSize = generationParams.GapSize;

        _dims = new int2(3, 2);
    }
    protected override void StartMeshGeneration()
    {
        _figureVertices = new NativeArray<VertexData>(SegmentsTotalVertexCount, Allocator.TempJob);
        _figureIndices = new NativeArray<short>(SegmentsTotalIndexCount, Allocator.TempJob);

        NativeArray<float3x2> lineSegments = new NativeArray<float3x2>(SegmentsTotalVertexCount, Allocator.TempJob);
        NativeArray<int2> quadStripsIndexers = new NativeArray<int2>(SegmentsCount, Allocator.TempJob);
        _quadStripsCollection = new QuadStripsBuffer(lineSegments, quadStripsIndexers);

        ValknutGenJob valknutGenJob = new ValknutGenJob()
        {
            P_InnerTriangleRadius = _innerTriangleRadius,
            P_Width = _width,
            P_GapSize = _gapSize,

            OutVertices = _figureVertices,
            OutIndices = _figureIndices,

            OutQuadStripsCollection = _quadStripsCollection
        };
        _figureMeshGenJobHandle = valknutGenJob.Schedule();

        _pointsRenderVertices = new NativeArray<float3>(PointRendererTotalVertexCount, Allocator.TempJob);
        _pointsRenderIndices = new NativeArray<short>(PointRendererTotalIndexCount, Allocator.TempJob);

        _pointsColliderVertices = new NativeArray<float3>(PointColliderTotalVertexCount, Allocator.TempJob);
        _pointsColliderIndices = new NativeArray<short>(PointColliderTotalIndexCount, Allocator.TempJob);

        ValknutGenJobSPM valknutSegmentPointGenJob = new ValknutGenJobSPM()
        {
            P_InnerTriangleRadius = _innerTriangleRadius,
            P_Width = _width,
            P_GapSize = _gapSize,
            P_Height = _segmentPointHeight,

            OutCollidersVertices = _pointsColliderVertices,
            OutCollidersIndices = _pointsColliderIndices,
            OutRenderVertices = _pointsRenderVertices,
            OutRenderIndices = _pointsRenderIndices
        };
        _segmentPointsMeshGenJobHandle = valknutSegmentPointGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();
    }

    protected override Figure GenerateFigureGameObject()
    {
        GameObject valknutGb = new GameObject("Valknut", typeof(Valknut), typeof(ValknutStatesController));
        valknutGb.layer = Layers.FigureLayer;
        Transform parentWheel = valknutGb.transform;

        GameObject segmentPointsParentGb = new GameObject("SegmentPoints");
        Transform segmentPointsParent = segmentPointsParentGb.transform;
        segmentPointsParent.parent = parentWheel;
        segmentPointsParent.SetSiblingIndex(0);

        GameObject segmentsParentGb = new GameObject("Segments");
        Transform segmentsParent = segmentsParentGb.transform;
        segmentsParent.parent = parentWheel;
        segmentsParent.SetSiblingIndex(1);

        _segments = new Array2D<ValknutSegment>(_dims);
        _segmentPoints = new Array2D<FigureSegmentPoint>(_dims);

        for (int triangle = 0; triangle < _dims.x; triangle++)
        {
            for (int part = 0; part < _dims.y; part++)
            {
                int2 index = new int2(triangle, part);

                GameObject segmentGb = Instantiate(_segmentPrefab);
                segmentGb.transform.parent = segmentsParent;
                ValknutSegment segment = segmentGb.AddComponent<ValknutSegment>();
                _segments[index] = segment;

                GameObject segmentPointGb = Instantiate(_segmentPointPrefab);
                segmentPointGb.layer = Layers.SegmentPointsLayer;
                segmentGb.name = "Segment";
                segmentPointGb.transform.parent = segmentPointsParent;
                FigureSegmentPoint segmentPoint = segmentPointGb.GetComponent<FigureSegmentPoint>();
                Assert.IsNotNull(segmentPoint);
                segmentPoint.Segment = segment;
                segmentPoint.AssignIndex(index);
                _segmentPoints[index] = segmentPoint;
            }
        }

        _valknut = valknutGb.GetComponent<Valknut>();
        return _valknut;
    }

    protected override void CompleteGeneration(FigureParamsSO figureParams)
    {
        _figureMeshGenJobHandle.Complete();
        _segmentPointsMeshGenJobHandle.Complete();

        int2 tasSegmentBuffersCount = new int2(SegmentVertexCountTAS, SegmentIndexCountTAS);
        int2 oasSegmentBuffersCount = new int2(SegmentVertexCountOAS, SegmentIndexCountOAS);
        MeshBuffersIndexers buffersData = new MeshBuffersIndexers();
        buffersData.Start = int2.zero;
        buffersData.Count = tasSegmentBuffersCount;

        int2 tasPointRendererBuffersCount = new int2(PointRendererVertexCountTAS, PointRendererIndexCountTAS);
        int2 oasPointRendererBuffersCount = new int2(PointRendererVertexCountOAS, PointRendererIndexCountOAS);
        MeshBuffersIndexers pointBuffersData = new MeshBuffersIndexers();
        pointBuffersData.Start = int2.zero;
        pointBuffersData.Count = tasPointRendererBuffersCount;

        int2 tasPointColliderBuffersCount = new int2(PointColliderVertexCountTAS, PointColliderIndexCountTAS);
        int2 oasPointColliderBuffersCount = new int2(PointColliderVertexCountOAS, PointColliderIndexCountOAS);
        MeshBuffersIndexers multiMeshBuffersData = new MeshBuffersIndexers();
        multiMeshBuffersData.Start = int2.zero;
        multiMeshBuffersData.Count = new int2(CubeVertexCount, CubeIndexCount);

        for (int triangle = 0; triangle < _dims.x; triangle++)
        {
            for (int part = 0; part < _dims.y; part++)
            {
                int2 index = new int2(triangle, part);

                UpdateSegment(_segments[index], buffersData, meshResPuzzleIndex: new int2(1, 0));
                Mesh[] multiMesh = CreateColliderMultiMesh(ref multiMeshBuffersData, part == 0);
                Mesh renderMesh = CreateSegmentPointRenderMesh(in pointBuffersData);
                _segmentPoints[index].InitializeWithMultiMesh(renderMesh, multiMesh);
                if (part == 0)
                {
                    buffersData.Start += tasSegmentBuffersCount;
                    buffersData.Count = oasSegmentBuffersCount;

                    pointBuffersData.Start += tasPointRendererBuffersCount;
                    pointBuffersData.Count = oasPointRendererBuffersCount;
                }
                else
                {
                    buffersData.Start += oasSegmentBuffersCount;
                    buffersData.Count = tasSegmentBuffersCount;

                    pointBuffersData.Start += oasPointRendererBuffersCount;
                    pointBuffersData.Count = tasPointRendererBuffersCount;
                }
            }
        }

        _valknut.Initialize(
            _segmentPoints,
            figureParams
        );

        _figureVertices.Dispose();
        _figureIndices.Dispose();

        _pointsRenderVertices.Dispose();
        _pointsRenderIndices.Dispose();
        _pointsColliderVertices.Dispose();
        _pointsColliderIndices.Dispose();
    }

    private Mesh[] CreateColliderMultiMesh(ref MeshBuffersIndexers buffersData, bool isTas)
    {
        int meshCount = isTas ? 3 : 2;
        Mesh[] meshes = new Mesh[meshCount];

        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i] = CreateSegmentPointColliderMesh(buffersData);
            buffersData.Start += buffersData.Count;
        }

        return meshes;
    }
}
