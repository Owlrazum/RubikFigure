using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

using Unity.Mathematics;
using Orazum.Collections;
using Orazum.Meshing;
using Orazum.Utilities.ConstContainers;

public class WheelGenerator : FigureGenerator
{
    [SerializeField]
    private FigureParamsSO _figureParams; 

    private NativeArray<WheelSegmentMesh> _segmentMeshes;

    private int _sideCount;
    private int _ringCount;
    private int _segmentResolution;

    private int _segmentCount;

    private MeshBuffersData _segmentBuffersData;
    private MeshBuffersData _segmentPointBuffersData;

    private float _innerRadius;
    private float _outerRadius;

    private GameObject _segmentPrefab;
    private GameObject _segmentPointPrefab;

    private Wheel _wheel;
    private WheelStatesController _wheelStatesController;
    private Array2D<WheelSegment> _segments;
    private Array2D<FigureSegmentPoint> _segmentPoints;

    private void Awake()
    {
        if (enabled)
        { 
            StartGeneration(_figureParams.FigureGenParamsSO);
        }
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
        
        _segmentBuffersData = new MeshBuffersData();
        _segmentBuffersData.Count = new int2(
            2 * (_segmentResolution + 1),
            6 * _segmentResolution
        );
        FigureSegment.InitializeVertexCount(_segmentBuffersData.Count.x);

        _segmentPointBuffersData = new MeshBuffersData();
        _segmentPointBuffersData.Count = new int2(
            _segmentBuffersData.Count.x * 2, 
            _segmentBuffersData.Count.y * 4 + 12
        );

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
        _segmentMeshes = new NativeArray<WheelSegmentMesh>(_ringCount, Allocator.TempJob);

        WheelGenJob wheelMeshGenJob = new WheelGenJob()
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
        _figureMeshGenJobHandle = wheelMeshGenJob.Schedule();

        _pointsRenderVertices = new NativeArray<float3>(_segmentPointBuffersData.Count.x * _ringCount, Allocator.TempJob);
        _pointsRenderIndices = new NativeArray<short>(_segmentPointBuffersData.Count.y * _ringCount, Allocator.TempJob);

        WheelSegmentPointMeshGenJob segmentPointMeshGenJob = new WheelSegmentPointMeshGenJob()
        {
            P_SideCount = _sideCount,
            P_RingCount = _ringCount,
            P_SegmentResolution = _segmentResolution,
            P_InnerCircleRadius = _innerRadius,
            P_OuterCircleRadius = _outerRadius,
            P_Height = _segmentPointHeight,

            OutputVertices = _pointsRenderVertices,
            OutputIndices = _pointsRenderIndices
        };
        _segmentPointsMeshGenJobHandle = segmentPointMeshGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();
    }

    protected override void GenerateFigureGameObject()
    {
        GameObject wheelGb = new GameObject("Wheel", typeof(Wheel), typeof(WheelStatesController));
        wheelGb.layer = LayerUtilities.FigureLayer;
        Transform parentWheel = wheelGb.transform;

        GameObject segmentPointsParentGb = new GameObject("SegmentPoints");
        Transform segmentPointsParent = segmentPointsParentGb.transform;
        segmentPointsParent.parent = parentWheel;
        segmentPointsParent.SetSiblingIndex(0);

        GameObject segmentsParentGb = new GameObject("Segments");
        Transform segmentsParent = segmentsParentGb.transform;
        segmentsParent.parent = parentWheel;
        segmentsParent.SetSiblingIndex(1);

        _segmentPoints = new Array2D<FigureSegmentPoint>(_sideCount, _ringCount);
        _segments = new Array2D<WheelSegment>(_sideCount, _ringCount);
        
        for (int ring = 0; ring < _ringCount; ring++)
        {
            for (int side = 0; side < _sideCount; side++)
            {
                GameObject segmentGb = Instantiate(_segmentPrefab);
                segmentGb.transform.parent = segmentsParent;
                WheelSegment segment = segmentGb.AddComponent<WheelSegment>();
                _segments[side, ring] = segment;

                GameObject segmentPointGb = Instantiate(_segmentPointPrefab);
                segmentPointGb.layer = LayerUtilities.SegmentPointsLayer;
                segmentPointGb.name = "Point[" + side + "," + ring + "]";
                segmentPointGb.transform.parent = segmentPointsParent;
                FigureSegmentPoint segmentPoint = segmentPointGb.GetComponent<FigureSegmentPoint>();
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

        _segmentBuffersData.Start = int2.zero;

        for (int side = 0; side < _sideCount; side++)
        {
            for (int ring = 0; ring < _ringCount; ring++)
            {
                UpdateSegment(_segments[side, ring], _segmentBuffersData, side);

                FigureSegmentPoint currentPoint = _segmentPoints[side, ring];
                currentPoint.InitializeWithSingleMesh(segmentPointMeshes[ring], _segments[side, ring], new int2(side, ring));

                _segmentBuffersData.Start += _segmentBuffersData.Count;
            }
        }

        _wheel.AssignSegmentMeshes(_segmentMeshes.ToArray());
        _wheel.Initialize(
            _segmentPoints, 
            _wheelStatesController,
            figureParams    
        );

        _figureVertices.Dispose();
        _figureIndices.Dispose();
        _segmentMeshes.Dispose();

        _pointsRenderVertices.Dispose();
        _pointsRenderIndices.Dispose();

        return _wheel;
    }

    private Mesh[] CreateSegmentPointMeshes()
    {
        Mesh[] meshes = new Mesh[_ringCount];

        _segmentPointBuffersData.Start = int2.zero;

        for (int i = 0; i < meshes.Length; i++)
        {
            Mesh segmentPointMesh = CreateSegmentPointRenderMesh(_segmentPointBuffersData);
            meshes[i] = segmentPointMesh;

            _segmentPointBuffersData.Start += _segmentPointBuffersData.Count;
        }

        return meshes;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CollectionUtilities.DisposeIfNeeded(_segmentMeshes);
    }
}