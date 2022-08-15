using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

using Unity.Mathematics;
using Orazum.Collections;
using Orazum.Utilities.ConstContainers;

public class WheelGenerationData
{
    public LevelDescriptionSO LevelDescription { get; set; }
    public int2[] EmtpySegmentPointIndicesForShuffle { get; set; }
    public SegmentVertexPositions[] SegmentVertexPositions { get; set; }

    public WheelGenerationData(
        Array2D<SegmentPoint> SegmentPointsArg, 
        int sideCountArg, 
        int ringCount)
    {
        SegmentPoints = SegmentPointsArg;
        SideCount = sideCountArg;
        RingCount = ringCount;
    }

    public Array2D<SegmentPoint> SegmentPoints { get; private set; }
    public int SideCount { get; private set; }
    public int RingCount { get; private set; }
}

public class WheelGenerator : MonoBehaviour
{
    [SerializeField]
    private LevelDescriptionSO _levelDescription;

    private WheelGenJob _wheelMeshGenJob;
    private JobHandle _wheelMeshGenJobHandle;

    private NativeArray<VertexData> _vertices;
    private NativeArray<short> _indices;

    private NativeArray<SegmentVertexPositions> _segmentPointsVertexPositions;

    private int _sideCount;
    private int _ringCount;
    private int _resolution;

    private int _vertexCount;
    private int _indexCount;

    private Wheel _wheel;
    private WheelStatesController _wheelController;
    private Array2D<Segment> _segments;
    private Array2D<SegmentPoint> _segmentPoints;


    private void Awake()
    {
        _sideCount = _levelDescription.GenerationParams.SideCount;
        _ringCount = _levelDescription.GenerationParams.RingCount;
        _resolution = _levelDescription.GenerationParams.SegmentResolution;
        int segmentCount = _sideCount * _ringCount;

        _vertexCount = 4 * _resolution;
        _indexCount = 6 * _resolution;

        Segment.InitializeVertexCount(_vertexCount);

        _vertices = new NativeArray<VertexData>(_vertexCount * segmentCount, Allocator.Persistent);
        _indices = new NativeArray<short>(_indexCount * segmentCount, Allocator.TempJob);
        _segmentPointsVertexPositions = new NativeArray<SegmentVertexPositions>(_ringCount, Allocator.TempJob);

        _wheelMeshGenJob = new WheelGenJob()
        {
            P_WheelHeight = _levelDescription.GenerationParams.Height,
            P_OuterCircleRadius = _levelDescription.GenerationParams.OuterRadius,
            P_InnerCircleRadius = _levelDescription.GenerationParams.InnerRadius,
            P_SideCount = _sideCount,
            P_RingCount = _ringCount,
            P_SegmentResolution = _levelDescription.GenerationParams.SegmentResolution,

            OutputVertices = _vertices,
            OutputIndices = _indices,

            OutputSegmentsVertexPositions = _segmentPointsVertexPositions
        };
        print($"{_levelDescription.GenerationParams.SegmentResolution}");
        _wheelMeshGenJobHandle = _wheelMeshGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();

        GameObject wheelGb = new GameObject("Wheel", typeof(Wheel), typeof(WheelStatesController));
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
                GameObject segmentGb = Instantiate(_levelDescription.SegmentPrefab);
                segmentGb.transform.parent = segmentsParent;
                Segment segment = segmentGb.GetComponent<Segment>();
                Assert.IsNotNull(segment);
                _segments[side, ring] = segment;

                GameObject segmentPointGb = Instantiate(_levelDescription.SegmentPointPrefab);
                segmentPointGb.layer = LayerUtilities.SEGMENT_POINTS_LAYER;
                segmentPointGb.name = "Point[" + side + "," + ring + "]";
                segmentPointGb.transform.parent = segmentPointsParent;
                SegmentPoint segmentPoint = segmentPointGb.GetComponent<SegmentPoint>();
                Assert.IsNotNull(segmentPoint);
                _segmentPoints[side, ring] = segmentPoint;
            }
        }

        _wheel = wheelGb.GetComponent<Wheel>();
        _wheelController = _wheel.GetComponent<WheelStatesController>();
    }

    private void Start()
    {
        _wheelMeshGenJobHandle.Complete();

        int vertexBufferStart = 0;
        int indexBufferStart = 0;

        for (int side = 0; side < _sideCount; side++)
        {
            for (int ring = 0; ring < _ringCount; ring++)
            {
                SegmentPoint currentPoint = _segmentPoints[side, ring];
                Segment currentSegment = _segments[side, ring];
                Mesh newMesh = currentSegment.MeshContainer.mesh;
                newMesh.MarkDynamic();

                newMesh.SetVertexBufferParams(_vertexCount, VertexData.VertexBufferMemoryLayout);
                newMesh.SetIndexBufferParams(_indexCount, IndexFormat.UInt16);

                newMesh.SetVertexBufferData(_vertices, vertexBufferStart, 0, _vertexCount, 0, SegmentMover.MESH_UPDATE_FLAGS);
                newMesh.SetIndexBufferData(_indices, indexBufferStart, 0, _indexCount, SegmentMover.MESH_UPDATE_FLAGS);

                newMesh.subMeshCount = 1;
                SubMeshDescriptor subMesh = new SubMeshDescriptor(
                    indexStart: 0,
                    indexCount: _indexCount
                );
                newMesh.SetSubMesh(0, subMesh);

                newMesh.RecalculateNormals();
                newMesh.RecalculateBounds();

                currentSegment.MeshContainer.mesh = newMesh;

                NativeArray<VertexData> segmentVertices = CollectionUtilities.GetSlice(
                    _vertices, vertexBufferStart, _vertexCount);
                currentSegment.Initialize(segmentVertices, side);

                currentPoint.InitializeAfterMeshesGenerated(currentSegment, new int2(side, ring));

                vertexBufferStart += _vertexCount;
                indexBufferStart += _indexCount;
            }
        }

        WheelGenerationData generationData = new WheelGenerationData(_segmentPoints, _sideCount, _ringCount);
        generationData.LevelDescription = _levelDescription;

        _wheel.GenerationInitialization(generationData);
        _wheelController.GenerationInitialization(_wheel, generationData);

        _vertices.Dispose();
        _indices.Dispose();
    }

    private void OnDestroy()
    {
        CollectionUtilities.DisposeIfNeeded(_vertices);
        CollectionUtilities.DisposeIfNeeded(_indices);
        CollectionUtilities.DisposeIfNeeded(_segmentPointsVertexPositions);
    }
}