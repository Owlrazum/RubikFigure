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

    private NativeArray<SegmentPointCornerPositions> _segmentPointsCornerPositions;

    private int _sideCount;
    private int _ringCount;

    private Wheel _wheel;
    private WheelStatesController _wheelController;
    private Array2D<Segment> _segments;
    private Array2D<SegmentPoint> _segmentPoints;


    private void Awake()
    {
        _sideCount = _levelDescription.GenerationParams.SideCount;
        _ringCount = _levelDescription.GenerationParams.RingCount;
        int segmentCount = _sideCount * _ringCount;

        _vertices = new NativeArray<VertexData>(SegmentMover.VERTEX_COUNT * segmentCount, Allocator.Persistent);
        _indices = new NativeArray<short>(SegmentMover.INDEX_COUNT * segmentCount, Allocator.TempJob);
        
        _segmentPointsCornerPositions = new NativeArray<SegmentPointCornerPositions>(segmentCount, Allocator.Persistent);

        _wheelMeshGenJob = new WheelGenJob()
        {
            P_WheelHeight = _levelDescription.GenerationParams.Height,
            P_OuterCircleRadius = _levelDescription.GenerationParams.OuterRadius,
            P_InnerCircleRadius = _levelDescription.GenerationParams.InnerRadius,
            P_SideCount = _sideCount,
            P_RingCount = _ringCount,
            
            OutputVertices = _vertices,
            OutputIndices = _indices,

            OutputSegmentPoints = _segmentPointsCornerPositions
        };
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

        _vertices = _wheelMeshGenJob.OutputVertices;
        _indices = _wheelMeshGenJob.OutputIndices;

        _segmentPointsCornerPositions = _wheelMeshGenJob.OutputSegmentPoints;

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

                newMesh.SetVertexBufferParams(SegmentMover.VERTEX_COUNT, VertexData.VertexBufferMemoryLayout);
                newMesh.SetIndexBufferParams(SegmentMover.INDEX_COUNT, IndexFormat.UInt16);

                newMesh.SetVertexBufferData(_vertices, vertexBufferStart, 0, SegmentMover.VERTEX_COUNT, 0, SegmentMover.MESH_UPDATE_FLAGS);
                newMesh.SetIndexBufferData(_indices, indexBufferStart, 0, SegmentMover.INDEX_COUNT, SegmentMover.MESH_UPDATE_FLAGS);

                newMesh.subMeshCount = 1;
                SubMeshDescriptor subMesh = new SubMeshDescriptor(
                    indexStart: 0,
                    indexCount: SegmentMover.INDEX_COUNT
                );
                newMesh.SetSubMesh(0, subMesh);

                newMesh.RecalculateNormals();
                newMesh.RecalculateBounds();

                currentSegment.MeshContainer.mesh = newMesh;

                NativeArray<VertexData> segmentVertices = CollectionUtilities.GetSlice(
                    _vertices, vertexBufferStart, SegmentMover.VERTEX_COUNT);
                currentSegment.Initialize(segmentVertices, side);

                int cornerIndex = side * _ringCount + ring;
                currentPoint.InitializeAfterMeshesGenerated(_segmentPointsCornerPositions[cornerIndex],
                    currentSegment, new int2(side, ring));

                vertexBufferStart += SegmentMover.VERTEX_COUNT;
                indexBufferStart += SegmentMover.INDEX_COUNT;
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
        CollectionUtilities.DisposeIfNeeded(_segmentPointsCornerPositions);
    }
}