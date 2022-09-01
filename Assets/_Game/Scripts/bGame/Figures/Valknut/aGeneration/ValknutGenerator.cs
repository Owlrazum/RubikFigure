using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Unity.Mathematics;
using Orazum.Constants;
using Orazum.Meshing;
using Orazum.Collections;

public class ValknutGenerator : FigureGenerator
{
    public const int MaxRangesCountForOneSegment = 7;

    private const int TrianglesCount = 3;
    private const int PartsCount = 2;

    private const int TotalRangesCount = (7 + 6) * 3 + (6 + 5) * 3;
    private const int TotalTransitionsCount = 2 * 3 + 2 * 3;

    private const int SegmentsCount = Valknut.TrianglesCount * Valknut.TriangleSegmentsCount; // three outer and three inner;
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

    private NativeArray<int2> _originTargetIndices;
    private QSTransitionsCollection _transitionsCollection;

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
        _figureVertices = new NativeArray<VertexData>(SegmentsTotalVertexCount, Allocator.TempJob);
        _figureIndices = new NativeArray<short>(SegmentsTotalIndexCount, Allocator.TempJob);

        NativeArray<float3x2> lineSegments = new NativeArray<float3x2>(SegmentsTotalVertexCount, Allocator.TempJob);
        NativeArray<int2> quadStripsIndexers = new NativeArray<int2>(SegmentsCount, Allocator.TempJob);
        _quadStripsCollection = new QuadStripsCollection(lineSegments, quadStripsIndexers);

        ValknutGenJob valknutGenJob = new ValknutGenJob()
        {
            P_InnerTriangleRadius = _innerTriangleRadius,
            P_Width = _width,
            P_GapSize = _gapSize,

            OutputVertices = _figureVertices,
            OutputIndices = _figureIndices,

            OutputQuadStripsCollection = _quadStripsCollection
        };
        _figureMeshGenJobHandle = valknutGenJob.Schedule();


        _originTargetIndices = new NativeArray<int2>(TotalTransitionsCount, Allocator.Persistent);
        NativeArray<int2> bufferIndexers = new NativeArray<int2>(TotalTransitionsCount, Allocator.Persistent);
        GenerateDataJobIndexData(originTargetIndices: ref _originTargetIndices, buffersIndexers: ref bufferIndexers);
        
        NativeArray<QSTransSegment> writeBuffer = new NativeArray<QSTransSegment>(TotalRangesCount, Allocator.Persistent);
        _transitionsCollection = new QSTransitionsCollection(writeBuffer, bufferIndexers);

        ValknutGenJobData transitionDataJob = new ValknutGenJobData()
        {
            InputQuadStripsCollection = _quadStripsCollection,
            InputOriginTargetIndices = _originTargetIndices,
            OutputTransitionsCollection = _transitionsCollection
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

    private void GenerateDataJobIndexData(ref NativeArray<int2> originTargetIndices, ref NativeArray<int2> buffersIndexers)
    {
        int2 originIndices = new int2(4, 5);
        int targetIndex = 0;
        
        int bufferStart = 0;
        int2 rangesCount = new int2(7, 6);

        int originTargetIndicesIndexer = 0;
        int buffersIndexersIndexer = 0;
        for (int i = 0; i < 6; i += 2)
        {
            originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.x, targetIndex);
            buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.x);
            bufferStart += rangesCount.x;

            originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.y, targetIndex);
            buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.y);
            bufferStart += rangesCount.y;

            originIndices.x = originIndices.x + 2 >= 6 ? 0 : originIndices.x + 2;
            originIndices.y = originIndices.y + 2 >= 6 ? 1 : originIndices.y + 2;
            targetIndex += 2;
        }

        targetIndex = 1;
        originIndices = new int2(5, 4);
        rangesCount = new int2(5, 6);
        for (int i = 0; i < 6; i += 2)
        {
            originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.x, targetIndex);
            buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.x);
            bufferStart += rangesCount.x;

            originTargetIndices[originTargetIndicesIndexer++] = new int2(originIndices.y, targetIndex);
            buffersIndexers[buffersIndexersIndexer++] = new int2(bufferStart, rangesCount.y);
            bufferStart += rangesCount.y;

            originIndices.x = originIndices.x + 2 >= 6 ? 1 : originIndices.x + 2;
            originIndices.y = originIndices.y + 2 >= 6 ? 0 : originIndices.y + 2;
            targetIndex += 2;
        }
    }

    protected override void GenerateFigureGameObject()
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
        _valknutStatesController = valknutGb.GetComponent<ValknutStatesController>();
    }

    private void Start()
    {
        FinishGeneration(_figureParams);
    }
    protected override FigureStatesController CompleteGeneration(FigureParamsSO figureParams)
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

        _dataJobHandle.Complete();

        Array2D<ValknutSegmentTransitions> transitionDatas =
            new Array2D<ValknutSegmentTransitions>(new int2(TrianglesCount, PartsCount));
        for (int i = 0; i < _transitionsCollection.QSTransSegsCount; i += 2)
        {
            QSTransition clockWiseTransition = _transitionsCollection.GetQSTransition(i);
            QSTransition antiClockWiseTransition = _transitionsCollection.GetQSTransition(i + 1);

            ValknutSegmentTransitions transData = new ValknutSegmentTransitions();
            ValknutSegmentTransitions.Clockwise(ref transData) = clockWiseTransition;
            ValknutSegmentTransitions.AntiClockwise(ref transData) = antiClockWiseTransition;

            int2 originTargetIndex = _originTargetIndices[i];
            int2 segmentIndex = new int2(originTargetIndex.y / PartsCount, originTargetIndex.y % PartsCount);
            transitionDatas[segmentIndex] = transData;
        }

        _valknut.AssignTransitionDatas(transitionDatas);
        _valknut.Initialize(
            _segmentPoints,
            _valknutStatesController,
            figureParams
        );

        _figureVertices.Dispose();
        _figureIndices.Dispose();

        _pointsRenderVertices.Dispose();
        _pointsRenderIndices.Dispose();
        _pointsColliderVertices.Dispose();
        _pointsColliderIndices.Dispose();

        return _valknutStatesController;
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

    protected override void OnDestroy()
    {
        base.OnDestroy();

        _transitionsCollection.DisposeIfNeeded();
        CollectionUtilities.DisposeIfNeeded(_originTargetIndices);
    }
}
