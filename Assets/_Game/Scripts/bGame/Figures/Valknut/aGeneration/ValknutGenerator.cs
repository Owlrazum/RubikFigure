using System.Collections.Generic;

using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

using Unity.Mathematics;
using Orazum.Utilities.ConstContainers;
using Orazum.Meshing;
using Orazum.Collections;

using Orazum.Math;
using static Orazum.Math.ClockOrderConversions;

public class ValknutGenerator : FigureGenerator
{
    public const int MaxRangesCountForOneSegment = 6;

    private const int TrianglesCount = 3;
    private const int PartsCount = 2;

    private const int SegmentsCount = Valknut.TrianglesCount * Valknut.TriangleSegmentsCount; // three outer and three inner;
    private const int SegmentQuadsCountTAS = 3;
    private const int SegmentQuadsCountOAS = 2;

    private const int SegmentVertexCountTAS = (SegmentQuadsCountTAS + 1) * 2;
    private const int SegmentVertexCountOAS = (SegmentQuadsCountOAS + 1) * 2;
    private const int SegmentIndexCountTAS = SegmentQuadsCountTAS * 6;
    private const int SegmentIndexCountOAS = SegmentQuadsCountOAS * 6;

    private const int SegmentTotalVertexCount = (SegmentVertexCountTAS + SegmentVertexCountOAS) * 3;
    private const int SegmentTotalIndexCount = (SegmentIndexCountTAS + SegmentIndexCountOAS) * 3;

    private const int PointRendererVertexCountTAS = SegmentVertexCountTAS * 2;
    private const int PointRendererVertexCountOAS = SegmentVertexCountOAS * 2;
    private const int PointRendererTotalVertexCount = SegmentTotalVertexCount * 2;

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

    [SerializeField]
    private FigureParamsSO _figureParams;

    private float _innerTriangleRadius;
    private float _width;
    private float _gapSize;

    private Valknut _valknut;
    private ValknutStatesController _valknutStatesController;

    private int2 _dims;

    private Array2D<ValknutSegment> _segments;
    private Array2D<FigureSegmentPoint> _segmentPoints;

    private JobHandle _dataJobHandle;

    private NativeArray<ValknutSegmentMesh> _segmentMeshes;
    private NativeArray<int3x2> _transitionDataJobIndexData;
    private NativeArray<float4x2> _transitionPositions;
    private NativeArray<float3> _lerpRanges;


    private void Awake()
    {
        StartGeneration(_figureParams.FigureGenParamsSO);
    }
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


        _transitionDataJobIndexData = new NativeArray<int3x2>(TotalTransitionsCount, Allocator.Persistent);
        _transitionPositions = new NativeArray<float4x2>(TotalRangesCount, Allocator.Persistent);
        _lerpRanges = new NativeArray<float3>(TotalRangesCount, Allocator.Persistent);
        GenerateDataJobIndexData();
        // string log = "";
        // for (int i = 0; i < TotalTransitionsCount; i++)
        // {
        //     log += $"{_dataJobIndexData[i][0]} {_dataJobIndexData[i][1]}\n";
        // }
        // Debug.Log(log);

        ValknutGenJobData transitionDataJob = new ValknutGenJobData()
        {
            InputSegmentMeshes = _segmentMeshes,
            InputIndexData = _transitionDataJobIndexData,
            OutputTransitionPositions = _transitionPositions,
            OutputLerpRanges = _lerpRanges
        };
        _dataJobHandle = transitionDataJob.ScheduleParallel(TotalTransitionsCount, 32, _figureMeshGenJobHandle);

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

            OutputCollidersVertices = _pointsColliderVertices,
            OutputCollidersIndices = _pointsColliderIndices,
            OutputRenderVertices = _pointsRenderVertices,
            OutputRenderIndices = _pointsRenderIndices
        };
        _segmentPointsMeshGenJobHandle = valknutSegmentPointGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();
    }

    private const int TotalRangesCount = (6 + 5) * 3 + (5 + 4) * 3;
    private const int TotalTransitionsCount = 2 * 3 + 2 * 3;

    private void GenerateDataJobIndexData()
    {
        int targetIndex = 0;
        int2 originIndices = new int2(4, 5);
        int2 rangesCount = new int2(6, 5);
        int rangeIndex = 0;
        int2 bufferStart = new int2(0, rangesCount.x);
        for (int i = 0; i < 6; i += 2)
        {
            _transitionDataJobIndexData[rangeIndex++] = new int3x2(
                new int3(originIndices.x, targetIndex, ClockOrderToInt(ClockOrderType.CW)),
                new int3(bufferStart.x, rangesCount.x, -1));
            _transitionDataJobIndexData[rangeIndex++] = new int3x2(
                new int3(originIndices.y, targetIndex, ClockOrderToInt(ClockOrderType.CCW)),
                new int3(bufferStart.y, rangesCount.y, -1));

            originIndices.x = originIndices.x + 2 >= 6 ? 0 : originIndices.x + 2;
            originIndices.y = originIndices.y + 2 >= 6 ? 1 : originIndices.y + 2;
            targetIndex += 2;

            bufferStart.x = bufferStart.y + rangesCount.y;
            bufferStart.y = bufferStart.x + rangesCount.x;
        }

        targetIndex = 1;
        originIndices = new int2(5, 4);
        rangesCount = new int2(4, 5);
        bufferStart.y -= 2;
        for (int i = 0; i < 6; i += 2)
        {
            _transitionDataJobIndexData[rangeIndex++] = new int3x2(
                new int3(originIndices.x, targetIndex, ClockOrderToInt(ClockOrderType.CW)),
                new int3(bufferStart.x, rangesCount.x, -1));
            _transitionDataJobIndexData[rangeIndex++] = new int3x2(
                new int3(originIndices.y, targetIndex, ClockOrderToInt(ClockOrderType.CCW)),
                new int3(bufferStart.y, rangesCount.y, -1));

            originIndices.x = originIndices.x + 2 >= 6 ? 1 : originIndices.x + 2;
            originIndices.y = originIndices.y + 2 >= 6 ? 0 : originIndices.y + 2;
            targetIndex += 2;

            bufferStart.x = bufferStart.y + rangesCount.y;
            bufferStart.y = bufferStart.x + rangesCount.x;
        }
    }

    protected override void GenerateFigureGameObject()
    {
        GameObject valknutGb = new GameObject("Valknut", typeof(Valknut), typeof(ValknutStatesController));
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
                segmentPointGb.layer = LayerUtilities.SegmentPointsLayer;
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
        _valknutStatesController = valknutGb.GetComponent<ValknutStatesController>();

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

        for (int triangle = 0; triangle < _dims.x; triangle++)
        {
            for (int part = 0; part < _dims.y; part++)
            {
                int2 index = new int2(triangle, part);

                UpdateSegment(_segments[index], buffersData, 0);
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

        _dataJobHandle.Complete();
        Debug.Log(LogUtilities.ToLog(_transitionPositions, LogUtilities.DecimalPlacesAfterDot.One, 5));
        Debug.Log(LogUtilities.ToLog(_lerpRanges, LogUtilities.DecimalPlacesAfterDot.Four, 10));

        Array2D<ValknutTransitionData> transitionDatas =
            new Array2D<ValknutTransitionData>(new int2(TrianglesCount, PartsCount));
        for (int i = 0; i < _transitionDataJobIndexData.Length; i += 2)
        {
            int3x2 index = int3x2.zero;

            index = _transitionDataJobIndexData[i];
            NativeArray<float4x2> _targetSegmentTransitionDataCW =
                _transitionPositions.GetSubArray(index[1].x, index[1].y);
            NativeArray<float3> _targetSegmentLerpRangesCW =
                _lerpRanges.GetSubArray(index[1].x, index[1].y);

            index = _transitionDataJobIndexData[i + 1];
            NativeArray<float4x2> _targetSegmentTransitionDataCCW =
                _transitionPositions.GetSubArray(index[1].x, index[1].y);
            NativeArray<float3> _targetSegmentLerpRangesCCW =
                _lerpRanges.GetSubArray(index[1].x, index[1].y);

            ValknutTransitionData transitionData = new ValknutTransitionData()
            {
                PositionsCW = _targetSegmentTransitionDataCW.AsReadOnly(),
                LerpRangesCW = _targetSegmentLerpRangesCW.AsReadOnly(),

                PositionsCCW = _targetSegmentTransitionDataCCW.AsReadOnly(),
                LerpRangesCCW = _targetSegmentLerpRangesCCW.AsReadOnly()
            };

            int2 segmentIndex = new int2((i / 2) / PartsCount, (i / 2) % PartsCount);
            transitionDatas[segmentIndex] = transitionData;
        }

        _valknut.AssignTransitionDatas(transitionDatas);
        _valknut.Initialize(
            _segmentPoints,
            _valknutStatesController,
            figureParams
        );

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

        CollectionUtilities.DisposeIfNeeded(_segmentMeshes);
        CollectionUtilities.DisposeIfNeeded(_transitionDataJobIndexData);
        CollectionUtilities.DisposeIfNeeded(_transitionPositions);
        CollectionUtilities.DisposeIfNeeded(_lerpRanges);
    }
}
