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
    private float2 _innerOuterRadii;

    private int _segmentCount;
    private int _segmentResolution;

    private MeshBuffersIndexers _segmentBuffersData;
    private MeshBuffersIndexers _segmentPointBuffersData;
    private int2 _quadStripCollectionData;

    private int2 MeshBuffersMaxCount;

    protected override void InitializeParameters(FigureGenParamsSO figureGenParams)
    {
        WheelGenParamsSO genParams = figureGenParams as WheelGenParamsSO;

        _dims = new int2(genParams.SideCount, genParams.RingCount);
        _segmentCount = _dims.x * _dims.y;
        _segmentResolution = genParams.SegmentResolution;

        MeshBuffersMaxCount = new int2(
            2 * (_segmentResolution + 1) * 2 + 10,
            6 * _segmentResolution * 2 + 30
        );

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

        _quadStripCollectionData.x = (_segmentResolution + 1) * _segmentCount;
        _quadStripCollectionData.y = _segmentCount;

        _innerOuterRadii = new float2(genParams.InnerRadius, genParams.OuterRadius);
    }

    protected override void StartMeshGeneration()
    {
        _figureVertices = new NativeArray<VertexData>(_segmentBuffersData.Count.x * _segmentCount, Allocator.TempJob);
        _figureIndices = new NativeArray<short>(_segmentBuffersData.Count.y * _segmentCount, Allocator.TempJob);

        NativeArray<float3x2> lineSegments = new NativeArray<float3x2>(_quadStripCollectionData.x, Allocator.Persistent);
        NativeArray<int2> quadStripsIndexers = new NativeArray<int2>(_quadStripCollectionData.y, Allocator.Persistent);
        _quadStripsCollection = new QuadStripsBuffer(lineSegments, quadStripsIndexers);
        _quadStripsCollection.Dims = _dims;

        WheelGenJob wheelMeshGenJob = new WheelGenJob()
        {
            P_SideCount = _dims.x,
            P_RingCount = _dims.y,
            P_SegmentResolution = _segmentResolution,
            P_InnerCircleRadius = _innerOuterRadii.x,
            P_OuterCircleRadius = _innerOuterRadii.y,

            OutVertices = _figureVertices,
            OutIndices = _figureIndices,
            OutQuadStripsCollection = _quadStripsCollection
        };
        _figureMeshGenJobHandle = wheelMeshGenJob.Schedule();

        _pointsRenderVertices = new NativeArray<float3>(_segmentPointBuffersData.Count.x * _dims.y, Allocator.TempJob);
        _pointsRenderIndices = new NativeArray<short>(_segmentPointBuffersData.Count.y * _dims.y, Allocator.TempJob);

        WheelGenJobSPM segmentPointMeshGenJob = new WheelGenJobSPM()
        {
            P_SideCount = _dims.x,
            P_RingCount = _dims.y,
            P_SegmentResolution = _segmentResolution,
            P_InnerCircleRadius = _innerOuterRadii.x,
            P_OuterCircleRadius = _innerOuterRadii.y,
            P_Height = SegmentPointHeight,

            OutputVertices = _pointsRenderVertices,
            OutputIndices = _pointsRenderIndices
        };
        _segmentPointsMeshGenJobHandle = segmentPointMeshGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();
    }

    protected override string FigureName => "Wheel";
    protected override GameObject GenerateFigureGb()
    {
        return new GameObject(FigureName, typeof(WheelStatesController), typeof(Wheel));
    }
    protected override FigureSegment AddSegmentComponent(GameObject segmentGb)
    {
        return segmentGb.AddComponent<FigureSegment>();
    }
    protected override FS_Point AddSegmentPointComponent(GameObject segmentPointGb)
    {
        return segmentPointGb.AddComponent<FS_Point>();
    }

    protected override void CompleteGeneration(FigureParamsSO figureParams, FigureGenParamsSO genParams)
    {
        _figureMeshGenJobHandle.Complete();
        _segmentPointsMeshGenJobHandle.Complete();

        Mesh[] segmentPointMeshes = CreateSegmentPointMeshes();

        _segmentBuffersData.Start = int2.zero;

        for (int side = 0; side < _dims.x; side++)
        {
            for (int ring = 0; ring < _dims.y; ring++)
            {
                UpdateSegment(_segments[side, ring], _segmentBuffersData, puzzleIndex: side, MeshBuffersMaxCount);

                FS_Point currentPoint = _segmentPoints[side, ring];
                currentPoint.InitializeWithSingleMesh(segmentPointMeshes[ring]);

                _segmentBuffersData.Start += _segmentBuffersData.Count;
            }
        }

        _figure.Initialize(
            _segmentPoints,
            figureParams,
            genParams
        );

        _figureVertices.Dispose();
        _figureIndices.Dispose();

        _pointsRenderVertices.Dispose();
        _pointsRenderIndices.Dispose();
    }
    private Mesh[] CreateSegmentPointMeshes()
    {
        Mesh[] meshes = new Mesh[_dims.y];

        _segmentPointBuffersData.Start = int2.zero;

        for (int i = 0; i < meshes.Length; i++)
        {
            Mesh segmentPointMesh = CreateSegmentPointRenderMesh(_segmentPointBuffersData);
            meshes[i] = segmentPointMesh;

            _segmentPointBuffersData.Start += _segmentPointBuffersData.Count;
        }

        return meshes;
    }
}