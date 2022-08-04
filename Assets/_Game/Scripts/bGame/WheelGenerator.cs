using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

using Orazum.Utilities;

public class WheelGenerationData
{
    public NativeArray<VertexData> Vertices { get; set; }
    public WheelSegment[] Segments {get; set;}
    public LevelDescriptionSO LevelDescriptionSO { get; set; }

    public WheelGenerationData(
        NativeArray<SegmentPoint> segmentPointsArg, 
        int sideCountArg, 
        int segmentCountInOneSideArg)
    {
        SegmentPoints = segmentPointsArg;
        SideCount = sideCountArg;
        SegmentCountInOneSide = segmentCountInOneSideArg;
    }

    public NativeArray<SegmentPoint> SegmentPoints { get; private set; }

    public int SideCount { get; private set; }
    public int SegmentCountInOneSide { get; private set; }
}

public class WheelGenerator : MonoBehaviour
{
    public const int VERTEX_COUNT_ONE_SEGMENT = 24;
    public const int INDEX_COUNT_ONE_SEGMENT = 36;
    public const MeshUpdateFlags MESH_UPDATE_FLAGS = MeshUpdateFlags.Default;

    [SerializeField]
    private WheelGenParamsSO _wheelGenParams;

    [SerializeField]
    private LevelDescriptionSO _levelDesc;

    private WheelGenJob _wheelMeshGenJob;
    private JobHandle _wheelMeshGenJobHandle;

    private NativeArray<VertexData> _vertices;
    private NativeArray<short> _indices;

    private NativeArray<SegmentPoint> _segmentPoints;

    private int _segmentCount;

    private Wheel _wheel;
    private WheelSegment[] _wheelSegments;

    private void Awake()
    {
        _segmentCount = _wheelGenParams.SegmentCountInOneSide * _wheelGenParams.SideCount;
        print("segment count " + _segmentCount);

        _vertices = new NativeArray<VertexData>(VERTEX_COUNT_ONE_SEGMENT * _segmentCount, Allocator.Persistent);
        _indices = new NativeArray<short>(INDEX_COUNT_ONE_SEGMENT * _segmentCount, Allocator.TempJob);
        
        _segmentPoints = new NativeArray<SegmentPoint>(_segmentCount, Allocator.Persistent);

        _wheelMeshGenJob = new WheelGenJob()
        {
            P_WheelHeight = _wheelGenParams.Height,
            P_OuterCircleRadius = _wheelGenParams.OuterRadius,
            P_InnerCircleRadius = _wheelGenParams.InnerRadius,
            P_SideCount = _wheelGenParams.SideCount,
            P_SegmentsCountInOneSide = _wheelGenParams.SegmentCountInOneSide,
            
            OutputVertices = _vertices,
            OutputIndices = _indices,

            OutputSegmentPoints = _segmentPoints
        };
        _wheelMeshGenJobHandle = _wheelMeshGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();

        _wheelSegments = new WheelSegment[_segmentCount];

        GameObject wheelGb = new GameObject("Wheel", typeof(Wheel));
        for (int i = 0; i < _segmentCount; i++)
        {
            GameObject wheelSegmentGb = new GameObject("WheelSegment", typeof(MeshFilter), typeof(MeshRenderer), typeof(WheelSegment));
            wheelSegmentGb.transform.parent = wheelGb.transform;

            _wheelSegments[i] = wheelSegmentGb.GetComponent<WheelSegment>();
        }

        _wheel = wheelGb.GetComponent<Wheel>();
    }

    private void Start()
    {
        _wheelMeshGenJobHandle.Complete();

        _vertices = _wheelMeshGenJob.OutputVertices;
        _indices = _wheelMeshGenJob.OutputIndices;

        int vertexBufferStart = 0;
        int indexBufferStart = 0;

        for (int i = 0; i < _segmentCount; i++)
        {
            Mesh newMesh = _wheelSegments[i].MeshContainer.mesh;
            newMesh.MarkDynamic();

            newMesh.SetVertexBufferParams(VERTEX_COUNT_ONE_SEGMENT, VertexData.VertexBufferMemoryLayout);
            newMesh.SetIndexBufferParams(INDEX_COUNT_ONE_SEGMENT, IndexFormat.UInt16);

            newMesh.SetVertexBufferData(_vertices, vertexBufferStart, 0, VERTEX_COUNT_ONE_SEGMENT, 0, MESH_UPDATE_FLAGS);
            newMesh.SetIndexBufferData(_indices, indexBufferStart, 0, INDEX_COUNT_ONE_SEGMENT, MESH_UPDATE_FLAGS);

            newMesh.subMeshCount = 1;
            SubMeshDescriptor subMesh = new SubMeshDescriptor(
                indexStart: 0,
                indexCount: INDEX_COUNT_ONE_SEGMENT
            );
            newMesh.SetSubMesh(0, subMesh);

            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();

            _wheelSegments[i].MeshContainer.mesh = newMesh;

            NativeArray<VertexData> segmentVertices = CollectionUtilities.GetSlice(
                _vertices, vertexBufferStart, VERTEX_COUNT_ONE_SEGMENT);
            _wheelSegments[i].Initialize(_wheelGenParams.MeshesMaterial, segmentVertices, i);

            vertexBufferStart += VERTEX_COUNT_ONE_SEGMENT;
            indexBufferStart += INDEX_COUNT_ONE_SEGMENT;
        }

        _segmentPoints = _wheelMeshGenJob.OutputSegmentPoints;

        WheelGenerationData wheelData = new WheelGenerationData(
            _segmentPoints, _wheelGenParams.SideCount, _wheelGenParams.SegmentCountInOneSide);
        wheelData.Vertices = _vertices;
        wheelData.Segments = _wheelSegments;
        wheelData.LevelDescriptionSO = _levelDesc;

        _wheel.GenerationInitialization(wheelData);

        _indices.Dispose();
    }

    private void OnDestroy()
    {
        CollectionUtilities.DisposeIfNeeded(_vertices);
        CollectionUtilities.DisposeIfNeeded(_indices);
        CollectionUtilities.DisposeIfNeeded(_segmentPoints);
    }
}