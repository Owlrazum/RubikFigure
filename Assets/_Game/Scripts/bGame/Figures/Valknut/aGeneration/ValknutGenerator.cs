using System.Collections.Generic;

using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

using Unity.Mathematics;
using Orazum.Utilities.ConstContainers;
using Orazum.Meshing;

public class ValknutGenerator : FigureGenerator
{
    private const int SegmentsCount = 3 + 3; // three outer and three inner;
    private const int SegmentQuadsCountTAS = 3;
    private const int SegmentQuadsCountOAS = 2;

    private const int SegmentVertexCountTAS = (SegmentQuadsCountTAS + 1) * 2;
    private const int SegmentVertexCountOAS = (SegmentQuadsCountOAS + 1) * 2;
    private const int SegmentIndexCountTAS  = SegmentQuadsCountTAS * 6;
    private const int SegmentIndexCountOAS  = SegmentQuadsCountOAS * 6;

    private const int SegmentTotalVertexCount = (SegmentVertexCountTAS + SegmentVertexCountOAS) * 3;
    private const int SegmentTotalIndexCount = (SegmentIndexCountTAS + SegmentIndexCountOAS) * 3;

    private const int PointRendererVertexCountTAS = SegmentVertexCountTAS * 2;
    private const int PointRendererVertexCountOAS = SegmentVertexCountOAS * 2;
    private const int PointRendererTotalVertexCount = SegmentTotalVertexCount * 2;

    private const int PointRendererIndexCountTAS = (SegmentQuadsCountTAS * 4 + 2) * 6;
    private const int PointRendererIndexCountOAS = (SegmentQuadsCountOAS * 4 + 2) * 6;
    private const int PointRendererTotalIndexCount =  (PointRendererIndexCountTAS + PointRendererIndexCountOAS) * 3;

    private const int CubeVertexCount = 8;
    private const int CubeIndexCount = 6 * 6;

    private const int PointColliderVertexCountTAS = SegmentQuadsCountTAS * CubeVertexCount;
    private const int PointColliderVertexCountOAS = SegmentQuadsCountOAS * CubeVertexCount;
    private const int PointColliderTotalVertexCount = (PointColliderVertexCountTAS + PointColliderVertexCountOAS) * 3;

    private const int PointColliderIndexCountTAS = SegmentQuadsCountTAS * CubeIndexCount;
    private const int PointColliderIndexCountOAS = SegmentQuadsCountOAS * CubeIndexCount;
    private const int PointColliderTotalIndexCount = (PointColliderIndexCountTAS + PointColliderIndexCountOAS) * 3;

    [SerializeField]
    private FigureParamsSO _figureParams;

    private float _innerTriangleRadius;
    private float _width;
    private float _gapSize;

    private GameObject _segmentPrefab;
    private GameObject _segmentPointPrefab;

    private Valknut _valknut;

    private List<ValknutSegment> _segments;
    private List<FigureSegmentPoint> _segmentPoints;

    private NativeArray<ValknutSegmentMesh> _segmentMeshes;

    private void Awake()
    {
        StartGeneration(_figureParams.FigureGenParamsSO);
    }
    protected override void InitializeParameters(FigureGenParamsSO figureGenParams)
    {
        ValknutGenParamsSO generationParams =  figureGenParams as ValknutGenParamsSO;
        
        _innerTriangleRadius = generationParams.InnerTriangleRadius;       
        _width = generationParams.Width;
        _gapSize = generationParams.GapSize;

        _segmentPointHeight = generationParams.Height;

        _segmentPrefab = generationParams.SegmentPrefab;
        _segmentPointPrefab = generationParams.SegmentPointPrefab;
    }
    protected override void StartMeshGeneration()
    {
        _segmentMeshes = new NativeArray<ValknutSegmentMesh>(SegmentsCount, Allocator.TempJob);
        _figureVertices = new NativeArray<VertexData>(SegmentTotalVertexCount, Allocator.TempJob);
        _figureIndices = new NativeArray<short>(SegmentTotalIndexCount, Allocator.TempJob);

        ValknutGenJob valknutGenJob = new ValknutGenJob()
        {
            P_InnerTriangleRadius = _innerTriangleRadius,
            P_Width = _width,
            P_GapSize = _gapSize,

            OutputVertices = _figureVertices,
            OutputIndices = _figureIndices,

            OutputSegmentMeshes = _segmentMeshes
        };
        _figureMeshGenJobHandle = valknutGenJob.Schedule();

        _pointsRenderVertices = new NativeArray<float3>(PointRendererTotalVertexCount, Allocator.TempJob);
        _pointsRenderIndices = new NativeArray<short>(PointRendererTotalIndexCount, Allocator.TempJob);

        _pointsColliderVertices = new NativeArray<float3>(PointColliderTotalVertexCount, Allocator.TempJob);
        _pointsColliderIndices = new NativeArray<short>(PointColliderTotalIndexCount, Allocator.TempJob);

        ValknutSegmentPointGenJob valknutSegmentPointGenJob = new ValknutSegmentPointGenJob()
        {
            P_InnerTriangleRadius = _innerTriangleRadius,
            P_Width = _width,
            P_GapSize = _gapSize,
            P_Height = _segmentPointHeight,

            OutputCollidersVertices = _pointsColliderVertices,
            OutputCollidersIndices  = _pointsColliderIndices,
            OutputRenderVertices    = _pointsRenderVertices,
            OutputRenderIndices     = _pointsRenderIndices 
        };
        _segmentPointsMeshGenJobHandle = valknutSegmentPointGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();
    }
    protected override void GenerateFigureGameObject()
    {
        GameObject valknutGb = new GameObject("Valknut", typeof(Valknut));
        valknutGb.layer = LayerUtilities.FigureLayer;
        Transform parentWheel = valknutGb.transform;

        GameObject segmentPointsParentGb = new GameObject("SegmentPoints");
        Transform segmentPointsParent = segmentPointsParentGb.transform;
        segmentPointsParent.parent = parentWheel;
        segmentPointsParent.SetSiblingIndex(0);

        GameObject segmentsParentGb = new GameObject("Segments");
        Transform segmentsParent = segmentsParentGb.transform;
        segmentsParent.parent = parentWheel;
        segmentsParent.SetSiblingIndex(1);

        _segments = new List<ValknutSegment>();
        _segmentPoints = new List<FigureSegmentPoint>();

        for (int segmentIndex = 0; segmentIndex < SegmentsCount; segmentIndex++)
        {
            GameObject segmentGb = Instantiate(_segmentPrefab);
            segmentGb.transform.parent = segmentsParent;
            ValknutSegment segment = segmentGb.AddComponent<ValknutSegment>();
            _segments.Add(segment);

            GameObject segmentPointGb = Instantiate(_segmentPointPrefab);
            segmentPointGb.layer = LayerUtilities.SegmentPointsLayer;
            segmentGb.name = "Segment";
            segmentPointGb.transform.parent = segmentPointsParent;
            FigureSegmentPoint segmentPoint = segmentPointGb.GetComponent<FigureSegmentPoint>();
            Assert.IsNotNull(segmentPoint);
            _segmentPoints.Add(segmentPoint);
            
        }

        _valknut = valknutGb.GetComponent<Valknut>();
    }

    private void Start()
    {
        FinishGeneration(_figureParams);
    }
    public override Figure FinishGeneration(FigureParamsSO figureParams)
    {
        _figureMeshGenJobHandle.Complete();
        _segmentPointsMeshGenJobHandle.Complete();

        int2 tasSegmentBuffersCount = new int2(SegmentVertexCountTAS, SegmentIndexCountTAS);
        int2 oasSegmentBuffersCount = new int2(SegmentVertexCountOAS, SegmentIndexCountOAS);
        MeshBuffersData buffersData = new MeshBuffersData();
        buffersData.Start = int2.zero;
        buffersData.Count = tasSegmentBuffersCount;

        int2 tasPointRendererBuffersCount = new int2(PointRendererVertexCountTAS, PointRendererIndexCountTAS);
        int2 oasPointRendererBuffersCount = new int2(PointRendererVertexCountOAS, PointRendererIndexCountOAS);
        MeshBuffersData pointBuffersData = new MeshBuffersData();
        pointBuffersData.Start = int2.zero;
        pointBuffersData.Count = tasPointRendererBuffersCount;

        int2 tasPointColliderBuffersCount = new int2(PointColliderVertexCountTAS, PointColliderIndexCountTAS);
        int2 oasPointColliderBuffersCount = new int2(PointColliderVertexCountOAS, PointColliderIndexCountOAS);
        MeshBuffersData multiMeshBuffersData = new MeshBuffersData();
        multiMeshBuffersData.Start = int2.zero;
        multiMeshBuffersData.Count = new int2(CubeVertexCount, CubeIndexCount);

        for (int i = 0; i < SegmentsCount; i++)
        { 
            UpdateSegment(_segments[i], buffersData, 0);
            Mesh[] multiMesh = CreateColliderMultiMesh(ref multiMeshBuffersData, i % 2 == 0);
            Mesh renderMesh = CreateSegmentPointRenderMesh(in pointBuffersData);
            _segmentPoints[i].InitializeWithMultiMesh(renderMesh, multiMesh, _segments[i], new int2(i, 0));
            if (i % 2 == 0)
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

        _figureVertices.Dispose();
        _figureIndices.Dispose();
        _segmentMeshes.Dispose();

        _pointsRenderVertices.Dispose();
        _pointsRenderIndices.Dispose();
        _pointsColliderVertices.Dispose();
        _pointsColliderIndices.Dispose();

        return _valknut;
    }

    private Mesh[] CreateColliderMultiMesh(ref MeshBuffersData buffersData, bool isTas)
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}
