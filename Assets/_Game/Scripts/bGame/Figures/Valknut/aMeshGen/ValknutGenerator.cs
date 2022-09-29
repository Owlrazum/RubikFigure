using Unity.Jobs;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Unity.Mathematics;
using Orazum.Constants;
using Orazum.Meshing;
using Orazum.Collections;

using static Orazum.Math.LineSegmentUtilities;

public class ValknutGenerator : FigureGenerator
{
    #region Constants
    private const int SegmentsCount = Valknut.TrianglesCount * Valknut.PartsCount; // three outer and three inner;
    private const int SegmentQuadsCountTAS = 3;
    private const int SegmentQuadsCountOAS = 2;

    private const int SegmentVertexCountTAS = (SegmentQuadsCountTAS + 1) * 2;
    private const int SegmentVertexCountOAS = (SegmentQuadsCountOAS + 1) * 2;
    private const int SegmentIndexCountTAS = SegmentQuadsCountTAS * 6;
    private const int SegmentIndexCountOAS = SegmentQuadsCountOAS * 6;

    private const int SegmentsTotalVertexCount = (SegmentVertexCountTAS + SegmentVertexCountOAS) * 3;
    private const int SegmentsTotalIndexCount = (SegmentIndexCountTAS + SegmentIndexCountOAS) * 3;

    private const int PointRendererVertexCountTAS = SegmentVertexCountTAS * 2;
    private const int PointRendererVertexCountOAS = SegmentVertexCountOAS * 2;
    private const int PointRendererTotalVertexCount = SegmentsTotalVertexCount * 2;

    private const int PointRendererIndexCountTAS = (SegmentQuadsCountTAS * 4 + 2) * 6;
    private const int PointRendererIndexCountOAS = (SegmentQuadsCountOAS * 4 + 2) * 6;
    private const int PointRendererTotalIndexCount = (PointRendererIndexCountTAS + PointRendererIndexCountOAS) * 3;

    private const int CubeVertexCount = 8;
    private const int CubeIndexCount = 6 * 6;

    private const int PointColliderVertexCountTAS = SegmentQuadsCountTAS * CubeVertexCount;
    private const int PointColliderVertexCountOAS = SegmentQuadsCountOAS * CubeVertexCount;
    private const int PointColliderTotalVertexCount = (PointColliderVertexCountTAS + PointColliderVertexCountOAS) * 3;

    private const int PointColliderIndexCountTAS = SegmentQuadsCountTAS * CubeIndexCount;
    private const int PointColliderIndexCountOAS = SegmentQuadsCountOAS * CubeIndexCount;
    private const int PointColliderTotalIndexCount = (PointColliderIndexCountTAS + PointColliderIndexCountOAS) * 3;


    private const int MaxRangesCountForOneSegment = 7;
    private const int MaxVertexCount = MaxRangesCountForOneSegment * 4;
    private const int MaxIndexCount = MaxRangesCountForOneSegment * 6;
    #endregion

    private float _innerTriangleRadius;
    private float _width;
    private float _gapSize;

    private Valknut _valknut;

    protected override void InitializeParameters(FigureGenParamsSO figureGenParams)
    {
        ValknutGenParamsSO generationParams = figureGenParams as ValknutGenParamsSO;

        _innerTriangleRadius = generationParams.InnerTriangleRadius;
        _width = generationParams.Width;
        _gapSize = generationParams.GapSize;

        _dims = new int2(3, 2);
    }
    protected override void StartMeshGeneration()
    {
        _figureVertices = new NativeArray<VertexData>(SegmentsTotalVertexCount, Allocator.TempJob);
        _figureIndices = new NativeArray<short>(SegmentsTotalIndexCount, Allocator.TempJob);

        _scaledFigureVertices = new NativeArray<VertexData>(SegmentsTotalVertexCount, Allocator.TempJob);

        NativeArray<float3x2> lineSegments = new NativeArray<float3x2>(SegmentsTotalVertexCount, Allocator.Persistent);
        NativeArray<int2> quadStripsIndexers = new NativeArray<int2>(SegmentsCount, Allocator.Persistent);
        _quadStripsCollection = new QuadStripsBuffer(lineSegments, quadStripsIndexers);

        ValknutGenJob valknutGenJob = new ValknutGenJob()
        {
            P_InnerTriangleRadius = _innerTriangleRadius,
            P_Width = _width,
            P_GapSize = _gapSize,

            OutVertices = _figureVertices,
            OutIndices = _figureIndices,

            OutQuadStripsCollection = _quadStripsCollection
        };
        _figureMeshGenJobHandle = valknutGenJob.Schedule();

        _pointsRenderVertices = new NativeArray<float3>(PointRendererTotalVertexCount, Allocator.TempJob);
        _pointsRenderIndices = new NativeArray<short>(PointRendererTotalIndexCount, Allocator.TempJob);

        _pointsColliderVertices = new NativeArray<float3>(PointColliderTotalVertexCount, Allocator.TempJob);
        _pointsColliderIndices = new NativeArray<short>(PointColliderTotalIndexCount, Allocator.TempJob);

        ValknutGenJobSPM valknutSegmentPointGenJob = new ValknutGenJobSPM()
        {
            P_InnerTriangleRadius = _innerTriangleRadius,
            P_Width = _width,
            P_GapSize = _gapSize,
            P_Height = SegmentPointHeight,

            OutCollidersVertices = _pointsColliderVertices,
            OutCollidersIndices = _pointsColliderIndices,
            OutRenderVertices = _pointsRenderVertices,
            OutRenderIndices = _pointsRenderIndices
        };
        _segmentPointsMeshGenJobHandle = valknutSegmentPointGenJob.Schedule();

        JobHandle.ScheduleBatchedJobs();
    }

    protected override string FigureName => "Valknut";
    protected override GameObject GenerateFigureGb()
    {
        return new GameObject(FigureName, typeof(Valknut), typeof(ValknutStatesController));
    }
    protected override FigureSegment AddSegmentComponent(GameObject segmentGb)
    {
        return segmentGb.AddComponent<FigureSegment>();
    }
    protected override FigureSegmentPoint AddSegmentPointComponent(GameObject segmentPointGb)
    {
        return segmentPointGb.AddComponent<ValknutSegmentPoint>();
    }

    protected override void CompleteGeneration(FigureParamsSO figureParams, FigureGenParamsSO genParams)
    {
        _figureMeshGenJobHandle.Complete();
        _segmentPointsMeshGenJobHandle.Complete();

        int2 tasSegmentBuffersCount = new int2(SegmentVertexCountTAS, SegmentIndexCountTAS);
        int2 oasSegmentBuffersCount = new int2(SegmentVertexCountOAS, SegmentIndexCountOAS);
        MeshBuffersIndexers buffersIndexers = new MeshBuffersIndexers();
        buffersIndexers.Start = int2.zero;
        buffersIndexers.Count = tasSegmentBuffersCount;

        int2 tasPointRendererBuffersCount = new int2(PointRendererVertexCountTAS, PointRendererIndexCountTAS);
        int2 oasPointRendererBuffersCount = new int2(PointRendererVertexCountOAS, PointRendererIndexCountOAS);
        MeshBuffersIndexers pointBuffersData = new MeshBuffersIndexers();
        pointBuffersData.Start = int2.zero;
        pointBuffersData.Count = tasPointRendererBuffersCount;

        int2 tasPointColliderBuffersCount = new int2(PointColliderVertexCountTAS, PointColliderIndexCountTAS);
        int2 oasPointColliderBuffersCount = new int2(PointColliderVertexCountOAS, PointColliderIndexCountOAS);
        MeshBuffersIndexers multiMeshBuffersData = new MeshBuffersIndexers();
        multiMeshBuffersData.Start = int2.zero;
        multiMeshBuffersData.Count = new int2(CubeVertexCount, CubeIndexCount);

        int quadStripIndexer = 0;
        for (int triangle = 0; triangle < _dims.x; triangle++)
        {
            for (int part = 0; part < _dims.y; part++)
            {
                int2 index = new int2(triangle, part);

                UpdateSegment(_segments[index], buffersIndexers, puzzleIndex: triangle, new int2(MaxVertexCount, MaxIndexCount));
                QuadStrip segmentStrip = _quadStripsCollection.GetQuadStrip(quadStripIndexer++);
                float3 start = GetLineSegmentCenter(segmentStrip[0]);
                float3 end = GetLineSegmentCenter(segmentStrip[segmentStrip.LineSegmentsCount - 1]);
                ValknutSegmentPoint valknutSegmentPoint = _segmentPoints[index] as ValknutSegmentPoint;
                valknutSegmentPoint.AssignEndPoints(start, end);

                Mesh[] multiMesh = CreateColliderMultiMesh(ref multiMeshBuffersData, part == 0);
                Mesh renderMesh = CreateSegmentPointRenderMesh(in pointBuffersData);
                _segmentPoints[index].InitializeWithMultiMesh(renderMesh, multiMesh);
                if (part == 0)
                {
                    buffersIndexers.Start += tasSegmentBuffersCount;
                    buffersIndexers.Count = oasSegmentBuffersCount;

                    pointBuffersData.Start += tasPointRendererBuffersCount;
                    pointBuffersData.Count = oasPointRendererBuffersCount;
                }
                else
                {
                    buffersIndexers.Start += oasSegmentBuffersCount;
                    buffersIndexers.Count = tasSegmentBuffersCount;

                    pointBuffersData.Start += oasPointRendererBuffersCount;
                    pointBuffersData.Count = tasPointRendererBuffersCount;
                }
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
        _pointsColliderVertices.Dispose();
        _pointsColliderIndices.Dispose();
    }

    private Mesh[] CreateColliderMultiMesh(ref MeshBuffersIndexers buffersData, bool isTas)
    {
        int meshCount = isTas ? 3 : 2;
        Mesh[] meshes = new Mesh[meshCount];

        for (int i = 0; i < meshes.Length; i++)
        {
            meshes[i] = CreateSegmentPointColliderMesh(buffersData);
            buffersData.Start += buffersData.Count;
        }

        return meshes;
    }
}
