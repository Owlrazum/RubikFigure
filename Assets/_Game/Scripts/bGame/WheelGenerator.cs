using UnityEngine;
using Unity.Mathematics;
using Unity.Jobs;
using UnityEngine.Rendering;

using Unity.Collections;

public class WheelGenerator : MonoBehaviour
{
    private const int VERTEX_COUNT_ONE_SEGMENT = 24;
    private const int INDEX_COUNT_ONE_SEGMENT = 36;

    [SerializeField]
    private WheelGenParamsSO _wheelGenParams;

    private WheelGenJob _wheelMeshGenJob;
    private JobHandle _wheelMeshGenJobHandle;

    private NativeArray<short> _vertexCounts;
    private NativeArray<short> _indexCounts;

    private NativeArray<VertexData> _vertices;
    private NativeArray<short> _indices;

    private int _segmentCount;
    private MeshUpdateFlags _meshUpdateFlags;

    private Wheel _wheel;


    private bool _needsDispose;

    private void Awake()
    {
        _segmentCount = _wheelGenParams.SegmentCountInOneSide * _wheelGenParams.SideCount;
        print("segment count " + _segmentCount);
        _meshUpdateFlags = MeshUpdateFlags.Default;

        Allocator allocator = Allocator.TempJob;

        _vertexCounts = new NativeArray<short>(_segmentCount, allocator);
        _indexCounts = new NativeArray<short>(_segmentCount, allocator);

        _vertices = new NativeArray<VertexData>(VERTEX_COUNT_ONE_SEGMENT * _segmentCount, allocator);
        _indices = new NativeArray<short>(INDEX_COUNT_ONE_SEGMENT * _segmentCount, allocator);

        _needsDispose = true;

        _wheelMeshGenJob = new WheelGenJob()
        {
            P_WheelHeight = _wheelGenParams.Height,
            P_OuterCircleRadius = _wheelGenParams.OuterRadius,
            P_InnerCircleRadius = _wheelGenParams.InnerRadius,
            P_SideCount = _wheelGenParams.SideCount,
            P_SegmentsCount = _wheelGenParams.SegmentCountInOneSide,
            
            OutputVertexCounts = _vertexCounts,
            OutputIndexCounts = _indexCounts,
            OutputVertices = _vertices,
            OutputIndices = _indices,
        };
        _wheelMeshGenJobHandle = _wheelMeshGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();

        MeshFilter[] segmentMeshFilters = new MeshFilter[_segmentCount];

        GameObject wheelGb = new GameObject("Wheel", typeof(Wheel));
        for (int i = 0; i < _segmentCount; i++)
        {
            GameObject wheelSegmentGb = new GameObject("WheelSegment", typeof(MeshFilter), typeof(MeshRenderer));
            wheelSegmentGb.transform.parent = wheelGb.transform;

            segmentMeshFilters[i] = wheelSegmentGb.GetComponent<MeshFilter>();
            wheelSegmentGb.GetComponent<MeshRenderer>().sharedMaterial = _wheelGenParams.MeshesMaterial;
        }

        _wheel = wheelGb.GetComponent<Wheel>();
        _wheel.Initialize(segmentMeshFilters);
    }

    private void Start()
    {
        _wheelMeshGenJobHandle.Complete();

        _vertexCounts = _wheelMeshGenJob.OutputVertexCounts;
        _indexCounts = _wheelMeshGenJob.OutputIndexCounts;

        _vertices = _wheelMeshGenJob.OutputVertices;
        _indices = _wheelMeshGenJob.OutputIndices;

        int vertexBufferStart = 0;
        int indexBufferStart = 0;

        for (int i = 0; i < _segmentCount; i++)
        {
            Mesh mesh = _wheel.GetMesh(i);
            mesh.MarkDynamic();

            mesh.SetVertexBufferParams(_wheelMeshGenJob.OutputVertexCounts[i], VertexData.VertexBufferMemoryLayout);
            mesh.SetIndexBufferParams(_wheelMeshGenJob.OutputIndexCounts[i], IndexFormat.UInt16);

            mesh.SetVertexBufferData(_vertices, vertexBufferStart, 0, _vertexCounts[i], 0, _meshUpdateFlags);
            mesh.SetIndexBufferData(_indices, indexBufferStart, 0, _indexCounts[i], _meshUpdateFlags);

            vertexBufferStart += _vertexCounts[i];
            indexBufferStart += _indexCounts[i];

            mesh.subMeshCount = 1;
            SubMeshDescriptor subMesh = new SubMeshDescriptor(
                indexStart: 0,
                indexCount: _indexCounts[i]
            );
            mesh.SetSubMesh(0, subMesh);

            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            _wheel.AssignMesh(mesh, i);
        }

        _vertexCounts.Dispose();
        _indexCounts.Dispose();

        _vertices.Dispose();
        _indices.Dispose();

        _needsDispose = false;
    }

    private void OnDestroy()
    {
        if (_needsDispose)
        { 
            _vertexCounts.Dispose();
            _indexCounts.Dispose();

            _vertices.Dispose();
            _indices.Dispose();
        }
    }
}