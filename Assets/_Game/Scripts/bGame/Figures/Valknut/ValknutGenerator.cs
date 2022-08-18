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
    private const int SEGMENTS_COUNT = 1;
    private const int QUADS_COUNT = 1;
    [SerializeField]
    private FigureParamsSO _figureParams; 

    private BuffersData _segmentBuffersData;
    private BuffersData _segmentPointBuffersData;

    private float _innerTriangleRadius;
    private float _width;
    private float _gapSize;

    private GameObject _segmentPrefab;
    private GameObject _segmentPointPrefab;

    private Valknut _valknut;

    private List<Segment> _segments;

    private void Awake()
    {
        _segments = new List<Segment>();
        StartGeneration(_figureParams.FigureGenParamsSO);
    }

    private void Start()
    {
        FinishGeneration(_figureParams);
    }

    protected override void InitializeParameters(FigureGenParamsSO figureGenParams)
    {
        ValknutGenParamsSO generationParams =  figureGenParams as ValknutGenParamsSO;
        _segmentBuffersData = new BuffersData();
        _segmentBuffersData.SetVertexCount(4 * QUADS_COUNT);
        _segmentBuffersData.SetIndexCount(6 * QUADS_COUNT);
        Segment.InitializeVertexCount(_segmentBuffersData.Count.x);

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
        _figureVertices = new NativeArray<VertexData>(_segmentBuffersData.Count.x * SEGMENTS_COUNT, Allocator.Persistent);
        _figureIndices = new NativeArray<short>(_segmentBuffersData.Count.y * SEGMENTS_COUNT, Allocator.TempJob);

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

        // for (int ring = 0; ring < _ringCount; ring++)
        // {
        //     for (int side = 0; side < _sideCount; side++)
        //     {
        GameObject segmentGb = Instantiate(_segmentPrefab);
        segmentGb.transform.parent = segmentsParent;
        Segment segment = segmentGb.GetComponent<Segment>();
        _segments.Add(segment);
        Assert.IsNotNull(segment);

        // GameObject segmentPointGb = Instantiate(_segmentPointPrefab);
        // segmentPointGb.layer = LayerUtilities.SEGMENT_POINTS_LAYER;
        // segmentPointGb.name = "Point[" + side + "," + ring + "]";
        // segmentPointGb.transform.parent = segmentPointsParent;
        // SegmentPoint segmentPoint = segmentPointGb.GetComponent<SegmentPoint>();
        // Assert.IsNotNull(segmentPoint);
        // _segmentPoints[side, ring] = segmentPoint;
        //     }
        // }

        _valknut = valknutGb.GetComponent<Valknut>();
    }

    public override Figure FinishGeneration(FigureParamsSO figureParams)
    {
        _figureMeshGenJobHandle.Complete();
        _segmentPointsMeshGenJobHandle.Complete();

        Mesh[] segmentPointMeshes = CreateSegmentPointMeshes();

        _segmentBuffersData.ResetVertexStart();
        _segmentBuffersData.ResetIndexStart();

        UpdateSegment(_segments[0], _segmentBuffersData, 0);

        _figureVertices.Dispose();
        _figureIndices.Dispose();

        // _segmentPointsVertices.Dispose();
        // _segmentPointsIndices.Dispose();

        return _valknut;
    }

    private Mesh[] CreateSegmentPointMeshes()
    {
        Mesh[] meshes = new Mesh[0];

        _segmentPointBuffersData.ResetVertexStart();
        _segmentPointBuffersData.ResetIndexStart();

        for (int i = 0; i < meshes.Length; i++)
        {
            Mesh segmentPointMesh = CreateSegmentPointMesh(_segmentPointBuffersData);
            meshes[i] = segmentPointMesh;

            _segmentPointBuffersData.AddVertexCountToVertexStart();
            _segmentPointBuffersData.AddIndexCountToIndexStart();
        }

        return meshes;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }
}