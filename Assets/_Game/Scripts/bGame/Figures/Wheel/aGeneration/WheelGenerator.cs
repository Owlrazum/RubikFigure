using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;
using Orazum.Meshing;
using Orazum.Constants;

public class WheelGenerator : FigureGenerator
{
    private const int LevitationTransSegCount = 3;
    private const int ClockOrderTransSegCount = 1;
    private const int VerticalTransSegCount = 2;

    [SerializeField]
    private FigureParamsSO _figureParams;

    private NativeArray<float3> _transitionGrid;

    private NativeArray<QSTransSegment> _atsi;
    private NativeArray<QSTransSegment> _ctsi;
    private NativeArray<QSTransSegment> _dtsi;
    private NativeArray<QSTransSegment> _utsi;
    private NativeArray<QSTransSegment> _levDtsi;
    private NativeArray<QSTransSegment> _levUtsi;

    private int2 _sidesRingsCount;
    private float2 _innerOuterRadii;

    private int _segmentCount;
    private int _segmentResolution;

    private MeshBuffersIndexers _segmentBuffersData;
    private MeshBuffersIndexers _segmentPointBuffersData;

    private Wheel _wheel;
    private WheelStatesController _wheelStatesController;

    private Array2D<WheelSegment> _segments;
    private Array2D<FigureSegmentPoint> _segmentPoints;

    private int3 _transSegsCount;

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
        base.InitializeParameters(figureGenParams);

        WheelGenParamsSO generationParams = figureGenParams as WheelGenParamsSO;
        _sidesRingsCount = new int2(generationParams.SideCount, generationParams.RingCount);

        _segmentCount = _sidesRingsCount.x * _sidesRingsCount.y;
        _segmentResolution = generationParams.SegmentResolution;

        _segmentBuffersData = new MeshBuffersIndexers();
        _segmentBuffersData.Count = new int2(
            2 * (_segmentResolution + 1),
            6 * _segmentResolution
        );

        _segmentPointBuffersData = new MeshBuffersIndexers();
        _segmentPointBuffersData.Count = new int2(
            _segmentBuffersData.Count.x * 2,
            _segmentBuffersData.Count.y * 4 + 12
        );

        _innerOuterRadii = new float2(generationParams.InnerRadius, generationParams.OuterRadius);

        _transSegsCount.x = VerticalTransSegCount * 2 * (_sidesRingsCount.y - 1) * _segmentResolution * _sidesRingsCount.x;
        _transSegsCount.y = LevitationTransSegCount * 2 * _sidesRingsCount.x;
        _transSegsCount.z = ClockOrderTransSegCount * 2 * _sidesRingsCount.x * _sidesRingsCount.y;
    }

    protected override void StartMeshGeneration()
    {
        _figureVertices = new NativeArray<VertexData>(_segmentBuffersData.Count.x * _segmentCount, Allocator.TempJob);
        _figureIndices = new NativeArray<short>(_segmentBuffersData.Count.y * _segmentCount, Allocator.TempJob);

        _transitionGrid = new NativeArray<float3>(
            _sidesRingsCount.x * (_sidesRingsCount.y + 1) * _segmentResolution, Allocator.TempJob);

        _dtsi = new NativeArray<QSTransSegment>(_transSegsCount.x, Allocator.Persistent);
        _utsi = new NativeArray<QSTransSegment>(_transSegsCount.x, Allocator.Persistent);

        _levDtsi = new NativeArray<QSTransSegment>(_transSegsCount.y, Allocator.Persistent);
        _levUtsi = new NativeArray<QSTransSegment>(_transSegsCount.y, Allocator.Persistent);

        _ctsi = new NativeArray<QSTransSegment>(_transSegsCount.z, Allocator.Persistent);
        _atsi = new NativeArray<QSTransSegment>(_transSegsCount.z, Allocator.Persistent);

        WheelGenJob wheelMeshGenJob = new WheelGenJob()
        {
            P_SideCount = _sidesRingsCount.x,
            P_RingCount = _sidesRingsCount.y,
            P_SegmentResolution = _segmentResolution,
            P_InnerCircleRadius = _innerOuterRadii.x,
            P_OuterCircleRadius = _innerOuterRadii.y,

            OutputVertices = _figureVertices,
            OutputIndices = _figureIndices,

            _HelperTransitionGrid = _transitionGrid,

            OutputDownTransSegments = _dtsi,
            OutputUpTransSegments = _utsi,
            OutputLevDownTransSegments = _levDtsi,
            OutputLevUpTransSegments = _levUtsi,
            OutputAntiCWTransSegments = _atsi,
            OutputCWTransSegments = _ctsi
        };
        _figureMeshGenJobHandle = wheelMeshGenJob.Schedule();

        _pointsRenderVertices = new NativeArray<float3>(_segmentPointBuffersData.Count.x * _sidesRingsCount.y, Allocator.TempJob);
        _pointsRenderIndices = new NativeArray<short>(_segmentPointBuffersData.Count.y * _sidesRingsCount.y, Allocator.TempJob);

        WheelSegmentPointMeshGenJob segmentPointMeshGenJob = new WheelSegmentPointMeshGenJob()
        {
            P_SideCount = _sidesRingsCount.x,
            P_RingCount = _sidesRingsCount.y,
            P_SegmentResolution = _segmentResolution,
            P_InnerCircleRadius = _innerOuterRadii.x,
            P_OuterCircleRadius = _innerOuterRadii.y,
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
        wheelGb.layer = Layers.FigureLayer;
        Transform parentWheel = wheelGb.transform;

        GameObject segmentPointsParentGb = new GameObject("SegmentPoints");
        Transform segmentPointsParent = segmentPointsParentGb.transform;
        segmentPointsParent.parent = parentWheel;
        segmentPointsParent.SetSiblingIndex(0);

        GameObject segmentsParentGb = new GameObject("Segments");
        Transform segmentsParent = segmentsParentGb.transform;
        segmentsParent.parent = parentWheel;
        segmentsParent.SetSiblingIndex(1);

        _segmentPoints = new Array2D<FigureSegmentPoint>(_sidesRingsCount.x, _sidesRingsCount.y);
        _segments = new Array2D<WheelSegment>(_sidesRingsCount.x, _sidesRingsCount.y);

        for (int ring = 0; ring < _sidesRingsCount.y; ring++)
        {
            for (int side = 0; side < _sidesRingsCount.x; side++)
            {
                int2 index = new int2(side, ring);

                GameObject segmentGb = Instantiate(_segmentPrefab);
                segmentGb.transform.parent = segmentsParent;
                WheelSegment segment = segmentGb.AddComponent<WheelSegment>();
                _segments[index] = segment;

                GameObject segmentPointGb = Instantiate(_segmentPointPrefab);
                segmentPointGb.layer = Layers.SegmentPointsLayer;
                segmentPointGb.name = "Point[" + side + "," + ring + "]";
                segmentPointGb.transform.parent = segmentPointsParent;
                FigureSegmentPoint segmentPoint = segmentPointGb.GetComponent<FigureSegmentPoint>();
                Assert.IsNotNull(segmentPoint);
                segmentPoint.Segment = segment;
                segmentPoint.AssignIndex(index);
                _segmentPoints[index] = segmentPoint;
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

        for (int side = 0; side < _sidesRingsCount.x; side++)
        {
            for (int ring = 0; ring < _sidesRingsCount.y; ring++)
            {
                UpdateSegment(_segments[side, ring], _segmentBuffersData, new int2(_segmentResolution, side));

                FigureSegmentPoint currentPoint = _segmentPoints[side, ring];
                currentPoint.InitializeWithSingleMesh(segmentPointMeshes[ring]);

                _segmentBuffersData.Start += _segmentBuffersData.Count;
            }
        }

        Array2D<WheelQSTransSegs> transitionDatas = new Array2D<WheelQSTransSegs>(_sidesRingsCount);
        int2x3 index = int2x3.zero;
        for (int side = 0; side < _sidesRingsCount.x; side++)
        {
            for (int ring = 0; ring < _sidesRingsCount.y; ring++)
            {
                NativeArray<QSTransSegment> atsi = _atsi.GetSubArray(index[0].x, ClockOrderTransSegCount);
                NativeArray<QSTransSegment> ctsi = _ctsi.GetSubArray(index[0].x, ClockOrderTransSegCount);
                index[0].x += ClockOrderTransSegCount;

                NativeArray<QSTransSegment> dtsi;
                if (ring == 0)
                { 
                    dtsi = _levDtsi.GetSubArray(index[1].x, LevitationTransSegCount);
                    index[1].x += LevitationTransSegCount;
                }
                else
                { 
                    dtsi = _dtsi.GetSubArray(index[2].x, VerticalTransSegCount * _segmentResolution);
                    index[2].x += VerticalTransSegCount * _segmentResolution;
                }

                NativeArray<QSTransSegment> utsi;
                if (ring == _sidesRingsCount.y - 1)
                {
                    utsi = _levUtsi.GetSubArray(index[1].y, LevitationTransSegCount);
                    index[1].y += LevitationTransSegCount;
                }
                else 
                {
                    utsi = _utsi.GetSubArray(index[2].y, VerticalTransSegCount * _segmentResolution);
                    index[2].y += VerticalTransSegCount * _segmentResolution;
                }

                WheelQSTransSegs transData = new WheelQSTransSegs()
                {
                    Atsi = atsi.AsReadOnly(),
                    Ctsi = ctsi.AsReadOnly(),
                    Dtsi = dtsi.AsReadOnly(),
                    Utsi = utsi.AsReadOnly()
                };

                transitionDatas[side, ring] = transData;
            }
        }
        _wheel.AssignTransitionDatas(transitionDatas);
        _wheel.Initialize(
            _segmentPoints,
            _wheelStatesController,
            figureParams
        );

        _figureVertices.Dispose();
        _figureIndices.Dispose();

        _pointsRenderVertices.Dispose();
        _pointsRenderIndices.Dispose();

        _transitionGrid.Dispose();

        return _wheel;
    }

    private Mesh[] CreateSegmentPointMeshes()
    {
        Mesh[] meshes = new Mesh[_sidesRingsCount.y];

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

        CollectionUtilities.DisposeIfNeeded(_transitionGrid);

        CollectionUtilities.DisposeIfNeeded(_atsi);
        CollectionUtilities.DisposeIfNeeded(_ctsi);
        CollectionUtilities.DisposeIfNeeded(_dtsi);
        CollectionUtilities.DisposeIfNeeded(_utsi);
        CollectionUtilities.DisposeIfNeeded(_levDtsi);
        CollectionUtilities.DisposeIfNeeded(_levUtsi);
    }
}