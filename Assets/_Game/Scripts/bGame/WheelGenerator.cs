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

    [SerializeField]
    private UIButton _shuffleButton;

    private WheelGenJob _wheelMeshGenJob;
    private JobHandle _wheelMeshGenJobHandle;

    private NativeArray<short> _vertexCounts;
    private NativeArray<short> _indexCounts;

    private NativeArray<VertexData> _vertices;
    private NativeArray<short> _indices;

    private NativeArray<float3> _circleRaysNativeArray;

    private int _segmentCount;
    private MeshUpdateFlags _meshUpdateFlags;

    private Wheel _wheel;
    private MeshFilter[] _wheelSegmentMeshFilters;

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

        _circleRaysNativeArray = new NativeArray<float3>(_wheelGenParams.SideCount, allocator);

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

            OutputCircleRays = _circleRaysNativeArray
        };
        _wheelMeshGenJobHandle = _wheelMeshGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();

        _wheelSegmentMeshFilters = new MeshFilter[_segmentCount];

        GameObject wheelGb = new GameObject("Wheel", typeof(Wheel));
        for (int i = 0; i < _segmentCount; i++)
        {
            GameObject wheelSegmentGb = new GameObject("WheelSegment", typeof(MeshFilter), typeof(MeshRenderer));
            wheelSegmentGb.transform.parent = wheelGb.transform;

            _wheelSegmentMeshFilters[i] = wheelSegmentGb.GetComponent<MeshFilter>();
            wheelSegmentGb.GetComponent<MeshRenderer>().sharedMaterial = _wheelGenParams.MeshesMaterial;
        }

        _wheel = wheelGb.GetComponent<Wheel>();
    }

    private void Start()
    {
        _wheelMeshGenJobHandle.Complete();

        // Debug.Log(
        //     "BBL " + _wheelMeshGenJob.BBL + "\n" +
        //     "BBR " + _wheelMeshGenJob.BBR + "\n" +
        //     "BTL " + _wheelMeshGenJob.BTL + "\n" +
        //     "BTR " + _wheelMeshGenJob.BTR + "\n" +
        //     "FBL " + _wheelMeshGenJob.FBL + "\n" +
        //     "FBR " + _wheelMeshGenJob.FBR + "\n" +
        //     "FTL " + _wheelMeshGenJob.FTL + "\n" +
        //     "FTR " + _wheelMeshGenJob.FTR
        // );

        _vertexCounts = _wheelMeshGenJob.OutputVertexCounts;
        _indexCounts = _wheelMeshGenJob.OutputIndexCounts;

        _vertices = _wheelMeshGenJob.OutputVertices;
        _indices = _wheelMeshGenJob.OutputIndices;

        _circleRaysNativeArray = _wheelMeshGenJob.OutputCircleRays;

        int vertexBufferStart = 0;
        int indexBufferStart = 0;

        for (int i = 0; i < _segmentCount; i++)
        {
            Mesh newMesh = _wheelSegmentMeshFilters[i].mesh;
            newMesh.MarkDynamic();

            newMesh.SetVertexBufferParams(_wheelMeshGenJob.OutputVertexCounts[i], VertexData.VertexBufferMemoryLayout);
            newMesh.SetIndexBufferParams(_wheelMeshGenJob.OutputIndexCounts[i], IndexFormat.UInt16);

            newMesh.SetVertexBufferData(_vertices, vertexBufferStart, 0, _vertexCounts[i], 0, _meshUpdateFlags);
            newMesh.SetIndexBufferData(_indices, indexBufferStart, 0, _indexCounts[i], _meshUpdateFlags);

            vertexBufferStart += _vertexCounts[i];
            indexBufferStart += _indexCounts[i];

            newMesh.subMeshCount = 1;
            SubMeshDescriptor subMesh = new SubMeshDescriptor(
                indexStart: 0,
                indexCount: _indexCounts[i]
            );
            newMesh.SetSubMesh(0, subMesh);

            newMesh.RecalculateNormals();
            newMesh.RecalculateBounds();

            _wheelSegmentMeshFilters[i].mesh = newMesh;
        }

        _wheel.MeshDataInit(_wheelSegmentMeshFilters, _wheelGenParams.SideCount, _wheelGenParams.SegmentCountInOneSide);

        CircleRays circleRays = new CircleRays(_circleRaysNativeArray);
        WheelUtilities.CurrentRays = circleRays;

        _wheel.GeneralInit(_shuffleButton);

        _vertexCounts.Dispose();
        _indexCounts.Dispose();

        _vertices.Dispose();
        _indices.Dispose();

        _circleRaysNativeArray.Dispose();

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