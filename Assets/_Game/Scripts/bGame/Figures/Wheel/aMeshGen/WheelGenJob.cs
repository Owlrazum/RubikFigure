using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using Orazum.Meshing;
using static Orazum.Constants.Math;

// [BurstCompile]
public struct WheelGenJob : IJob
{
    public int P_SideCount;
    public int P_RingCount;
    public int P_SegmentResolution;

    public float P_InnerCircleRadius;
    public float P_OuterCircleRadius;

    [WriteOnly]
    public NativeArray<VertexData> OutVertices;

    [WriteOnly]
    public NativeArray<short> OutIndices;

    [WriteOnly]
    public QuadStripsBuffer OutQuadStripsCollection;

    private MeshBuffersIndexers _buffersData;

    private float3 _segStartRay;
    private float3 _sideStartRay;
    private float3x2 _normalAndUV;

    private float3 _radiiData;

    private quaternion _segQuadRotDelta;

    private int2 _quadStripsCollectionIndexer;
    private int _quadStripIndexer;

    public void Execute()
    {
        _buffersData = new MeshBuffersIndexers();

        _segStartRay = new float3(math.cos(TAU / 4), 0, math.sin(TAU / 4));
        _normalAndUV = new float3x2(
            math.up(),
            float3.zero
        );

        _segQuadRotDelta = quaternion.AxisAngle(math.up(), TAU / P_SideCount / P_SegmentResolution);

        _radiiData.xy = new float2(P_InnerCircleRadius, P_OuterCircleRadius);
        _radiiData.z = (P_OuterCircleRadius - P_InnerCircleRadius) / P_RingCount;

        float deltaUV = 1.0f / P_SideCount;
        float startUV = 1 - deltaUV / 2;

        int2 offsets = new int2(P_SegmentResolution, P_SegmentResolution * P_SideCount);

        for (int side = 0; side < P_SideCount; side++)
        {
            _normalAndUV[1] = new float3(0, startUV - side * deltaUV, 0);

            _radiiData.x = P_InnerCircleRadius;
            _radiiData.y = _radiiData.x + _radiiData.z;

            quaternion sideRot = quaternion.AxisAngle(math.up(), TAU / P_SideCount * side);
            _sideStartRay = math.rotate(sideRot, _segStartRay);

            for (int ring = 0; ring < P_RingCount; ring++)
            {
                AddSegment(side, ring, offsets);

                _radiiData.x = _radiiData.y;
                _radiiData.y += _radiiData.z;
            }
        }
    }

    private void AddSegment(int side, int ring, int2 offsets)
    {
        _buffersData.LocalCount = int2.zero;

        QuadStripBuilder quadStripBuilder =
            new QuadStripBuilder(OutVertices, OutIndices, _normalAndUV);

        float3 currentRay = _sideStartRay;
        float3x2 quadStripSegment = new float3x2(
            (currentRay * _radiiData.x),
            (currentRay * _radiiData.y));
        quadStripBuilder.Start(quadStripSegment, ref _buffersData);

        _quadStripsCollectionIndexer.y = P_SegmentResolution + 1;
        NativeArray<float3x2> lineSegments = 
            OutQuadStripsCollection.GetBufferSegmentAndWriteIndexer(_quadStripsCollectionIndexer, _quadStripIndexer++);
        _quadStripsCollectionIndexer.x += P_SegmentResolution + 1;
        lineSegments[0] = quadStripSegment;

        int gridIndexer = side * offsets.x + ring * offsets.y;
        for (int i = 0; i < P_SegmentResolution; i++)
        {
            currentRay = math.rotate(_segQuadRotDelta, currentRay);
            quadStripSegment[0] = (currentRay * _radiiData.x);
            quadStripSegment[1] = (currentRay * _radiiData.y);
            quadStripBuilder.Continue(quadStripSegment, ref _buffersData);
            lineSegments[i + 1] = quadStripSegment;
        }
    }
}