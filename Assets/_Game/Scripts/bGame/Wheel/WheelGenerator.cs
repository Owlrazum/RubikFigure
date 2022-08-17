using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

using Unity.Mathematics;
using Orazum.Collections;
using Orazum.Utilities.ConstContainers;

public class WheelGenerator : FigureGenerator
{
    [SerializeField]
    private FigureParamsSO _figureParams; 
    private WheelGenJob _wheelMeshGenJob;
    private SegmentPointMeshGenJob _segmentPointMeshGenJob;

    private NativeArray<SegmentMesh> _segmentMeshes;

    private int _sideCount;
    private int _ringCount;
    private int _segmentResolution;

    private int _segmentCount;

    private BuffersData _segmentBuffersData;
    private BuffersData _segmentPointBuffersData;

    private float _innerRadius;
    private float _outerRadius;

    private GameObject _segmentPrefab;
    private GameObject _segmentPointPrefab;

    private Wheel _wheel;
    private WheelStatesController _wheelStatesController;
    private Array2D<Segment> _segments;
    private Array2D<SegmentPoint> _segmentPoints;

    private void Awake()
    {
        StartGeneration(_figureParams.FigureGenParamsSO);
    }

    private void Start()
    {
        FinishGeneration(_figureParams);
    }

    protected override void InitializeParameters(FigureGenParamsSO figureGenParams)
    {
        WheelGenParamsSO generationParams =  figureGenParams as WheelGenParamsSO;
        _sideCount = generationParams.SideCount;
        _ringCount = generationParams.RingCount;
        _segmentResolution = generationParams.SegmentResolution;
        
        _segmentCount = _sideCount * _ringCount;
        
        _segmentBuffersData = new BuffersData();
        _segmentBuffersData.SetVertexCount( 4 * _segmentResolution);
        _segmentBuffersData.SetIndexCount(6 * _segmentResolution);
        Segment.InitializeVertexCount(_segmentBuffersData.Count.x);

        _segmentPointBuffersData = new BuffersData();
        _segmentPointBuffersData.SetVertexCount(_segmentBuffersData.Count.x * 4 + 8);
        _segmentPointBuffersData.SetIndexCount(_segmentBuffersData.Count.y * 4 + 12);

        _innerRadius = generationParams.InnerRadius;
        _outerRadius = generationParams.OuterRadius;

        _segmentPointHeight = generationParams.Height;

        _segmentPrefab = generationParams.SegmentPrefab;
        _segmentPointPrefab = generationParams.SegmentPointPrefab;
    }

    protected override void StartMeshGeneration()
    {
        _figureVertices = new NativeArray<VertexData>(_segmentBuffersData.Count.x * _segmentCount, Allocator.Persistent);
        _figureIndices = new NativeArray<short>(_segmentBuffersData.Count.y * _segmentCount, Allocator.TempJob);
        _segmentMeshes = new NativeArray<SegmentMesh>(_ringCount, Allocator.TempJob);

        _wheelMeshGenJob = new WheelGenJob()
        {
            P_SideCount = _sideCount,
            P_RingCount = _ringCount,
            P_SegmentResolution = _segmentResolution,
            P_InnerCircleRadius = _innerRadius,
            P_OuterCircleRadius = _outerRadius,

            OutputVertices = _figureVertices,
            OutputIndices = _figureIndices,

            OutputSegmentMeshes = _segmentMeshes
        };
        _figureMeshGenJobHandle = _wheelMeshGenJob.Schedule();

        _segmentPointsVertices = new NativeArray<float3>(_segmentPointBuffersData.Count.x * _ringCount, Allocator.TempJob);
        _segmentPointsIndices = new NativeArray<short>(_segmentPointBuffersData.Count.y * _ringCount, Allocator.TempJob);

        _segmentPointMeshGenJob = new SegmentPointMeshGenJob()
        {
            P_SideCount = _sideCount,
            P_RingCount = _ringCount,
            P_SegmentResolution = _segmentResolution,
            P_InnerCircleRadius = _innerRadius,
            P_OuterCircleRadius = _outerRadius,
            P_Height = _segmentPointHeight,

            OutputVertices = _segmentPointsVertices,
            OutputIndices = _segmentPointsIndices
        };
        _segmentPointsMeshGenJobHandle = _segmentPointMeshGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();
    }

    protected override void GenerateFigureGameObject()
    {
        GameObject wheelGb = new GameObject("Wheel", typeof(Wheel), typeof(WheelStatesController));
        wheelGb.layer = LayerUtilities.WHEEL_LAYER;
        Transform parentWheel = wheelGb.transform;

        GameObject segmentPointsParentGb = new GameObject("SegmentPoints");
        Transform segmentPointsParent = segmentPointsParentGb.transform;
        segmentPointsParent.parent = parentWheel;
        segmentPointsParent.SetSiblingIndex(0);

        GameObject segmentsParentGb = new GameObject("Segments");
        Transform segmentsParent = segmentsParentGb.transform;
        segmentsParent.parent = parentWheel;
        segmentsParent.SetSiblingIndex(1);

        _segmentPoints = new Array2D<SegmentPoint>(_sideCount, _ringCount);
        _segments = new Array2D<Segment>(_sideCount, _ringCount);
        
        for (int ring = 0; ring < _ringCount; ring++)
        {
            for (int side = 0; side < _sideCount; side++)
            {
                GameObject segmentGb = Instantiate(_segmentPrefab);
                segmentGb.transform.parent = segmentsParent;
                Segment segment = segmentGb.GetComponent<Segment>();
                Assert.IsNotNull(segment);
                _segments[side, ring] = segment;

                GameObject segmentPointGb = Instantiate(_segmentPointPrefab);
                segmentPointGb.layer = LayerUtilities.SEGMENT_POINTS_LAYER;
                segmentPointGb.name = "Point[" + side + "," + ring + "]";
                segmentPointGb.transform.parent = segmentPointsParent;
                SegmentPoint segmentPoint = segmentPointGb.GetComponent<SegmentPoint>();
                Assert.IsNotNull(segmentPoint);
                _segmentPoints[side, ring] = segmentPoint;
            }
        }

        _wheel = wheelGb.GetComponent<Wheel>();
        _wheelStatesController = _wheel.GetComponent<WheelStatesController>();
    }

    public override Figure FinishGeneration(FigureParamsSO figureParams)
    {
        _figureMeshGenJobHandle.Complete();
        _segmentPointsMeshGenJobHandle.Complete();

        Mesh[] segmentPointMeshes = CreateSegmentPointMeshes();

        _segmentBuffersData.ResetVertexStart();
        _segmentBuffersData.ResetIndexStart();

        for (int side = 0; side < _sideCount; side++)
        {
            for (int ring = 0; ring < _ringCount; ring++)
            {
                UpdateSegment(_segments[side, ring], _segmentBuffersData, side);

                SegmentPoint currentPoint = _segmentPoints[side, ring];
                currentPoint.InitializeAfterMeshesGenerated(segmentPointMeshes[ring], _segments[side, ring], new int2(side, ring));

                _segmentBuffersData.AddVertexCountToVertexStart();
                _segmentBuffersData.AddIndexCountToIndexStart();
            }
        }

        _wheel.Initialize(
            _segmentPoints, 
            _segmentMeshes.ToArray(), 
            _wheelStatesController,
            figureParams    
        );

        _figureVertices.Dispose();
        _figureIndices.Dispose();
        _segmentMeshes.Dispose();

        _segmentPointsVertices.Dispose();
        _segmentPointsIndices.Dispose();

        return _wheel;
    }

    private Mesh[] CreateSegmentPointMeshes()
    {
        Mesh[] meshes = new Mesh[_ringCount];

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
        CollectionUtilities.DisposeIfNeeded(_segmentMeshes);
    }
}