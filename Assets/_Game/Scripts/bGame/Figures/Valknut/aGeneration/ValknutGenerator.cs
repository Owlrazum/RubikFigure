using System.Collections.Generic;

using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

using Unity.Mathematics;
using Orazum.Collections;
using Orazum.Utilities.ConstContainers;

public class ValknutGenerator : FigureGenerator
{
    private const int SEGMENTS_COUNT = 3 + 3; // three outer and three inner;
    private const int QUADS_COUNT_TWO_ANGLE_SEGMENT = 3;
    private const int QUADS_COUNT_ONE_ANGLE_SEGMENT = 2;

    private const int VERTEX_COUNT_TWO_ANGLE_SEGMNET = QUADS_COUNT_TWO_ANGLE_SEGMENT * 4;
    private const int INDEX_COUNT_TWO_ANGLE_SEGMNET  = QUADS_COUNT_TWO_ANGLE_SEGMENT * 6;

    private const int VERTEX_COUNT_ONE_ANGLE_SEGMNET = QUADS_COUNT_ONE_ANGLE_SEGMENT * 4;
    private const int INDEX_COUNT_ONE_ANGLE_SEGMNET  = QUADS_COUNT_ONE_ANGLE_SEGMENT * 6;

    private const int VERTEX_COUNT_VALKNUT_TRIANGLE = VERTEX_COUNT_TWO_ANGLE_SEGMNET + VERTEX_COUNT_ONE_ANGLE_SEGMNET;
    private const int INDEX_COUNT_VALKNUT_TRIANGLE = INDEX_COUNT_TWO_ANGLE_SEGMNET + INDEX_COUNT_ONE_ANGLE_SEGMNET;

    private const int TOTAL_VERTEX_COUNT = VERTEX_COUNT_VALKNUT_TRIANGLE * 3;
    private const int TOTAL_INDEX_COUNT = INDEX_COUNT_VALKNUT_TRIANGLE * 3;

    [SerializeField]
    private FigureParamsSO _figureParams;

    private float _innerTriangleRadius;
    private float _width;
    private float _gapSize;

    private GameObject _segmentPrefab;
    private GameObject _segmentPointPrefab;

    private Valknut _valknut;

    private List<FigureSegment> _segments;

    private void Awake()
    {
        _segments = new List<FigureSegment>();
        StartGeneration(_figureParams.FigureGenParamsSO);
    }

    private void Start()
    {
        FinishGeneration(_figureParams);
    }

    protected override void InitializeParameters(FigureGenParamsSO figureGenParams)
    {
        ValknutGenParamsSO generationParams =  figureGenParams as ValknutGenParamsSO;
        // _segmentBuffersData.SetVertexCount(4 * QUADS_COUNT);
        // _segmentBuffersData.SetIndexCount(6 * QUADS_COUNT);
        // Segment.InitializeVertexCount(_segmentBuffersData.Count.x);

        // _segmentPointBuffersData = new BuffersData();
        // _segmentPointBuffersData.SetVertexCount(_segmentBuffersData.Count.x * 4 + 8);
        // _segmentPointBuffersData.SetIndexCount(_segmentBuffersData.Count.y * 4 + 12);

        _innerTriangleRadius = generationParams.InnerTriangleRadius;       
        _width = generationParams.Width;
        _gapSize = generationParams.GapSize;

        _segmentPrefab = generationParams.SegmentPrefab;
        _segmentPointPrefab = generationParams.SegmentPointPrefab;
    }

    protected override void StartMeshGeneration()
    {
        _figureVertices = new NativeArray<VertexData>(TOTAL_VERTEX_COUNT, Allocator.Persistent);
        _figureIndices = new NativeArray<short>(TOTAL_INDEX_COUNT, Allocator.TempJob);

        ValknutGenJob valknutGenJob = new ValknutGenJob()
        {
            P_InnerTriangleRadius = _innerTriangleRadius,
            P_Width = _width,
            P_GapSize = _gapSize,

            OutputVertices = _figureVertices,
            OutputIndices = _figureIndices
        };
        _figureMeshGenJobHandle = valknutGenJob.Schedule();

        // _segmentPointsVertices = new NativeArray<float3>(_segmentPointBuffersData.Count.x * _ringCount, Allocator.TempJob);
        // _segmentPointsIndices = new NativeArray<short>(_segmentPointBuffersData.Count.y * _ringCount, Allocator.TempJob);

        // _segmentPointMeshGenJob = new SegmentPointMeshGenJob()
        // {
        //     P_SideCount = _sideCount,
        //     P_RingCount = _ringCount,
        //     P_SegmentResolution = _segmentResolution,
        //     P_InnerCircleRadius = _innerRadius,
        //     P_OuterCircleRadius = _outerRadius,
        //     P_Height = _segmentPointHeight,

        //     OutputVertices = _segmentPointsVertices,
        //     OutputIndices = _segmentPointsIndices
        // };
        // _segmentPointsMeshGenJobHandle = _segmentPointMeshGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();
    }

    protected override void GenerateFigureGameObject()
    {
        GameObject valknutGb = new GameObject("Valknut", typeof(Valknut));
        valknutGb.layer = LayerUtilities.FIGURE_LAYER;
        Transform parentWheel = valknutGb.transform;

        GameObject segmentPointsParentGb = new GameObject("SegmentPoints");
        Transform segmentPointsParent = segmentPointsParentGb.transform;
        segmentPointsParent.parent = parentWheel;
        segmentPointsParent.SetSiblingIndex(0);

        GameObject segmentsParentGb = new GameObject("Segments");
        Transform segmentsParent = segmentsParentGb.transform;
        segmentsParent.parent = parentWheel;
        segmentsParent.SetSiblingIndex(1);

        // _segmentPoints = new Array2D<SegmentPoint>(_sideCount, _ringCount);
        // _segments = new Array2D<Segment>(_sideCount, _ringCount);

        for (int segmentIndex = 0; segmentIndex < SEGMENTS_COUNT; segmentIndex++)
        {
            GameObject segmentGb = Instantiate(_segmentPrefab);
            segmentGb.transform.parent = segmentsParent;
            FigureSegment segment = segmentGb.AddComponent<ValknutSegment>();
            _segments.Add(segment);
            Assert.IsNotNull(segment);
        }

        // GameObject segmentPointGb = Instantiate(_segmentPointPrefab);
        // segmentPointGb.layer = LayerUtilities.SEGMENT_POINTS_LAYER;
        // segmentPointGb.name = "Point[" + side + "," + ring + "]";
        // segmentPointGb.transform.parent = segmentPointsParent;
        // SegmentPoint segmentPoint = segmentPointGb.GetComponent<SegmentPoint>();
        // Assert.IsNotNull(segmentPoint);
        // _segmentPoints[side, ring] = segmentPoint;

        _valknut = valknutGb.GetComponent<Valknut>();
    }

    public override Figure FinishGeneration(FigureParamsSO figureParams)
    {
        _figureMeshGenJobHandle.Complete();
        _segmentPointsMeshGenJobHandle.Complete();

        Mesh[] segmentPointMeshes = CreateSegmentPointMeshes();

        BuffersData segmentBuffersData = new BuffersData();
        segmentBuffersData.ResetVertexStart();
        segmentBuffersData.ResetIndexStart();
        segmentBuffersData.SetVertexCount(VERTEX_COUNT_TWO_ANGLE_SEGMNET);
        segmentBuffersData.SetIndexCount(INDEX_COUNT_TWO_ANGLE_SEGMNET);

        for (int i = 0; i < SEGMENTS_COUNT; i++)
        { 
            UpdateSegment(_segments[i], segmentBuffersData, 0);
            if (i % 2 == 0)
            {
                segmentBuffersData.AddToVertexStart(VERTEX_COUNT_TWO_ANGLE_SEGMNET);
                segmentBuffersData.AddToIndexStart(INDEX_COUNT_TWO_ANGLE_SEGMNET);
                segmentBuffersData.SetVertexCount(VERTEX_COUNT_ONE_ANGLE_SEGMNET);
                segmentBuffersData.SetIndexCount(INDEX_COUNT_ONE_ANGLE_SEGMNET);
            }
            else
            { 
                segmentBuffersData.AddToVertexStart(VERTEX_COUNT_ONE_ANGLE_SEGMNET);
                segmentBuffersData.AddToIndexStart(INDEX_COUNT_ONE_ANGLE_SEGMNET);
                segmentBuffersData.SetVertexCount(VERTEX_COUNT_TWO_ANGLE_SEGMNET);
                segmentBuffersData.SetIndexCount(INDEX_COUNT_TWO_ANGLE_SEGMNET);
            }
        }

        _figureVertices.Dispose();
        _figureIndices.Dispose();

        // _segmentPointsVertices.Dispose();
        // _segmentPointsIndices.Dispose();

        return _valknut;
    }

    private Mesh[] CreateSegmentPointMeshes()
    {
        Mesh[] meshes = new Mesh[0];

        // _segmentPointBuffersData.ResetVertexStart();
        // _segmentPointBuffersData.ResetIndexStart();

        // for (int i = 0; i < meshes.Length; i++)
        // {
        //     Mesh segmentPointMesh = CreateSegmentPointMesh(_segmentPointBuffersData);
        //     meshes[i] = segmentPointMesh;

        //     _segmentPointBuffersData.AddVertexCountToVertexStart();
        //     _segmentPointBuffersData.AddIndexCountToIndexStart();
        // }

        return meshes;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}