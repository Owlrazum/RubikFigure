using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

using Unity.Mathematics;
using Orazum.Utilities;

public class WheelGenerationData
{
    public Segment[,] Segments {get; set;}
    public LevelDescriptionSO LevelDescriptionSO { get; set; }
    public Material EmptyMaterial { get; set; }
    public Material HighlightMaterial { get; set; }

    public WheelGenerationData(
        NativeArray<SegmentPointCornerPositions> SegmentPointCornerPositionsArg, 
        int sideCountArg, 
        int segmentCountInOneSideArg)
    {
        SegmentPointCornerPositions = SegmentPointCornerPositionsArg;
        SideCount = sideCountArg;
        SegmentCountInOneSide = segmentCountInOneSideArg;
    }

    public NativeArray<SegmentPointCornerPositions> SegmentPointCornerPositions { get; private set; }
    public int SideCount { get; private set; }
    public int SegmentCountInOneSide { get; private set; }
}

public class WheelGenerator : MonoBehaviour
{
    [SerializeField]
    private WheelGenParamsSO _wheelGenParams;

    [SerializeField]
    private LevelDescriptionSO _levelDesc;

    private WheelGenJob _wheelMeshGenJob;
    private JobHandle _wheelMeshGenJobHandle;

    private NativeArray<VertexData> _vertices;
    private NativeArray<short> _indices;

    private NativeArray<SegmentPointCornerPositions> _segmentPoints;

    private int _segmentCountInOneSide;
    private int _sideCount;

    private Wheel _wheel;
    private WheelController _wheelController;
    private Segment[,] _segments;

    private void Awake()
    {
        _sideCount = _wheelGenParams.SideCount;
        _segmentCountInOneSide = _wheelGenParams.SegmentCountInOneSide;
        int segmentCount = _sideCount * _segmentCountInOneSide;

        _vertices = new NativeArray<VertexData>(Segment.VERTEX_COUNT * segmentCount, Allocator.Persistent);
        _indices = new NativeArray<short>(Segment.INDEX_COUNT * segmentCount, Allocator.TempJob);
        
        _segmentPoints = new NativeArray<SegmentPointCornerPositions>(segmentCount, Allocator.Persistent);

        _wheelMeshGenJob = new WheelGenJob()
        {
            P_WheelHeight = _wheelGenParams.Height,
            P_OuterCircleRadius = _wheelGenParams.OuterRadius,
            P_InnerCircleRadius = _wheelGenParams.InnerRadius,
            P_SideCount = _sideCount,
            P_SegmentsCountInOneSide = _segmentCountInOneSide,
            
            OutputVertices = _vertices,
            OutputIndices = _indices,

            OutputSegmentPoints = _segmentPoints
        };
        _wheelMeshGenJobHandle = _wheelMeshGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();

        _segments = new Segment[_segmentCountInOneSide, _sideCount];

        GameObject wheelGb = new GameObject("Wheel", typeof(Wheel), typeof(WheelController));
        for (int row = 0; row < _sideCount; row++)
        {
            for (int col = 0; col < _segmentCountInOneSide; col++)
            {
                GameObject wheelSegmentGb = new GameObject("WheelSegment", typeof(MeshFilter), typeof(MeshRenderer), typeof(Segment));
                wheelSegmentGb.transform.parent = wheelGb.transform;

                _segments[col, row] = wheelSegmentGb.GetComponent<Segment>();
            }
        }

        _wheel = wheelGb.GetComponent<Wheel>();
        _wheelController = _wheel.GetComponent<WheelController>();
    }

    private void Start()
    {
        _wheelMeshGenJobHandle.Complete();

        _vertices = _wheelMeshGenJob.OutputVertices;
        _indices = _wheelMeshGenJob.OutputIndices;

        int vertexBufferStart = 0;
        int indexBufferStart = 0;

        for (int row = 0; row < _sideCount; row++)
        {
            for (int col = 0; col < _segmentCountInOneSide; col++)
            { 
                Mesh newMesh = _segments[col, row].MeshContainer.mesh;
                newMesh.MarkDynamic();

                newMesh.SetVertexBufferParams(Segment.VERTEX_COUNT, VertexData.VertexBufferMemoryLayout);
                newMesh.SetIndexBufferParams(Segment.INDEX_COUNT, IndexFormat.UInt16);

                newMesh.SetVertexBufferData(_vertices, vertexBufferStart, 0, Segment.VERTEX_COUNT, 0, Segment.MESH_UPDATE_FLAGS);
                newMesh.SetIndexBufferData(_indices, indexBufferStart, 0, Segment.INDEX_COUNT, Segment.MESH_UPDATE_FLAGS);

                newMesh.subMeshCount = 1;
                SubMeshDescriptor subMesh = new SubMeshDescriptor(
                    indexStart: 0,
                    indexCount: Segment.INDEX_COUNT
                );
                newMesh.SetSubMesh(0, subMesh);

                newMesh.RecalculateNormals();
                newMesh.RecalculateBounds();

                _segments[col, row].MeshContainer.mesh = newMesh;

                NativeArray<VertexData> segmentVertices = CollectionUtilities.GetSlice(
                    _vertices, vertexBufferStart, Segment.VERTEX_COUNT);
                _segments[col, row].Initialize(
                    _wheelGenParams.MeshesMaterial, 
                    segmentVertices, 
                    new int2(col, row),
                    row
                );

                vertexBufferStart += Segment.VERTEX_COUNT;
                indexBufferStart += Segment.INDEX_COUNT;
            }
        }

        _segmentPoints = _wheelMeshGenJob.OutputSegmentPoints;

        WheelGenerationData generationData = new WheelGenerationData(
            _segmentPoints, _wheelGenParams.SideCount, _wheelGenParams.SegmentCountInOneSide);
        generationData.Segments = _segments;
        generationData.LevelDescriptionSO = _levelDesc;

        generationData.EmptyMaterial = _wheelGenParams.EmptyMaterial;
        generationData.HighlightMaterial = _wheelGenParams.HighlightMaterial;

        _wheel.GenerationInitialization(generationData);
        _wheelController.GenerationInitialization(_wheel, generationData);

        _vertices.Dispose();
        _indices.Dispose();

    }

    private void OnDestroy()
    {
        CollectionUtilities.DisposeIfNeeded(_vertices);
        CollectionUtilities.DisposeIfNeeded(_indices);
        CollectionUtilities.DisposeIfNeeded(_segmentPoints);
    }
}