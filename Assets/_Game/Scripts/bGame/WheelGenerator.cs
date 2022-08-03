using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Rendering;

using Orazum.Utilities;

public class WheelData
{
    public NativeArray<VertexData> Vertices { get; set; }

    public WheelData(
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
    private UIButton _shuffleButton;

    private WheelGenJob _wheelMeshGenJob;
    private JobHandle _wheelMeshGenJobHandle;

    private NativeArray<short> _vertexCounts;
    private NativeArray<short> _indexCounts;

    private NativeArray<VertexData> _vertices;
    private NativeArray<short> _indices;

    private NativeArray<SegmentPoint> _segmentPoints;

    private int _segmentCount;

    private Wheel _wheel;
    private WheelSegment[] _wheelSegments;

    private bool _needsDispose;

    private void Awake()
    {
        _segmentCount = _wheelGenParams.SegmentCountInOneSide * _wheelGenParams.SideCount;
        print("segment count " + _segmentCount);

        _vertexCounts = new NativeArray<short>(_segmentCount, Allocator.TempJob);
        _indexCounts = new NativeArray<short>(_segmentCount, Allocator.TempJob);

        _vertices = new NativeArray<VertexData>(VERTEX_COUNT_ONE_SEGMENT * _segmentCount, Allocator.Persistent);
        _indices = new NativeArray<short>(INDEX_COUNT_ONE_SEGMENT * _segmentCount, Allocator.TempJob);
        
        _segmentPoints = new NativeArray<SegmentPoint>(_segmentCount, Allocator.Persistent);

        _needsDispose = true;

        _wheelMeshGenJob = new WheelGenJob()
        {
            P_WheelHeight = _wheelGenParams.Height,
            P_OuterCircleRadius = _wheelGenParams.OuterRadius,
            P_InnerCircleRadius = _wheelGenParams.InnerRadius,
            P_SideCount = _wheelGenParams.SideCount,
            P_SegmentsCountInOneSide = _wheelGenParams.SegmentCountInOneSide,
            
            OutputVertexCounts = _vertexCounts,
            OutputIndexCounts = _indexCounts,
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

        _vertexCounts = _wheelMeshGenJob.OutputVertexCounts;
        _indexCounts = _wheelMeshGenJob.OutputIndexCounts;

        _vertices = _wheelMeshGenJob.OutputVertices;
        _indices = _wheelMeshGenJob.OutputIndices;

        _segmentPoints = _wheelMeshGenJob.OutputSegmentPoints;

        WheelData wheelData = new WheelData(
            _segmentPoints, _wheelGenParams.SideCount, _wheelGenParams.SegmentCountInOneSide);
        wheelData.Vertices = _vertices;
        _wheel.AssignData(wheelData);

        int vertexBufferStart = 0;
        int indexBufferStart = 0;

        for (int i = 0; i < _segmentCount; i++)
        {
            Mesh newMesh = _wheelSegments[i].MeshContainer.mesh;
            newMesh.MarkDynamic();

            newMesh.SetVertexBufferParams(_wheelMeshGenJob.OutputVertexCounts[i], VertexData.VertexBufferMemoryLayout);
            newMesh.SetIndexBufferParams(_wheelMeshGenJob.OutputIndexCounts[i], IndexFormat.UInt16);

            newMesh.SetVertexBufferData(_vertices, vertexBufferStart, 0, _vertexCounts[i], 0, MESH_UPDATE_FLAGS);
            newMesh.SetIndexBufferData(_indices, indexBufferStart, 0, _indexCounts[i], MESH_UPDATE_FLAGS);

            newMesh.subMeshCount = 1;
            SubMeshDescriptor subMesh = new SubMeshDescriptor(
                indexStart: 0,
                indexCount: _indexCounts[i]
            );
            newMesh.SetSubMesh(0, subMesh);

            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();

            _wheelSegments[i].MeshContainer.mesh = newMesh;

            NativeArray<VertexData> segmentVertices = CollectionsUtilities.GetVerticesSlice(
                _vertices, vertexBufferStart, _vertexCounts[i]);
            _wheelSegments[i].Initialize(_wheelGenParams.MeshesMaterial, segmentVertices, i);

            vertexBufferStart += _vertexCounts[i];
            indexBufferStart += _indexCounts[i];
        }

        _wheel.GenerationInitialization(_wheelSegments, _shuffleButton);

        _vertexCounts.Dispose();
        _indexCounts.Dispose();

        _indices.Dispose();

        _needsDispose = false;
    }

    private void OnDestroy()
    {
        if (_needsDispose)
        { 
            _vertexCounts.Dispose();
            _indexCounts.Dispose();

            _indices.Dispose();
        }

        _segmentPoints.Dispose();
        _vertices.Dispose();
    }
}