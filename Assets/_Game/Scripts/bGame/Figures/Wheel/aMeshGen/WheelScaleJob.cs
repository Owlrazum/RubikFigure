using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using Orazum.Meshing;
using static Orazum.Constants.Math;

[BurstCompile]
public struct WheelScaleJob : IJob
{
    public int2 P_Index;
    public float P_ScaleValue;

    public int P_SideCount;
    public int P_RingCount;
    public int P_SegmentResolution;

    public float P_InnerCircleRadius;
    public float P_OuterCircleRadius;

    public float2 P_UV;

    [WriteOnly]
    public NativeArray<VertexData> OutVertices;

    [WriteOnly]
    public NativeArray<short> OutIndices;

    public MeshBuffersIndexersForJob OutBuffersIndexers;

    private int2 _quadStripsCollectionIndexer;
    private int _quadStripIndexer;

    public void Execute()
    {
        MeshBuffersIndexers buffersIndexers = OutBuffersIndexers.GetIndexersForChangesInsideJob();

        float3 segStartRay = new float3(math.cos(TAU / 4), 0, math.sin(TAU / 4));
        // float sideStartAngle = TAU / P_SideCount * P_Index.x;
        // float scaleAngle = TAU * P_ScaleValue / P_SideCount;
        // sideStartAngle -= scaleAngle / 2;
        float scaleDelta = P_ScaleValue - 1;
        float sideStartAngle = TAU / P_SideCount * P_Index.x * (1 - scaleDelta / 2);
        quaternion sideRot = quaternion.AxisAngle(math.up(), sideStartAngle);
        segStartRay = math.rotate(sideRot, segStartRay);

        float radiusDelta = (P_OuterCircleRadius - P_InnerCircleRadius) / P_RingCount;
        float2 radii = new float2(P_InnerCircleRadius + P_Index.y * radiusDelta, 0);
        radii.y = radii.x + radiusDelta;
        radii.x /= P_ScaleValue;
        radii.y *= P_ScaleValue;

        float lineSegmentRotAngle = TAU / P_SideCount / P_SegmentResolution * P_ScaleValue;
        quaternion segQuadRotDelta = quaternion.AxisAngle(math.up(), lineSegmentRotAngle);

        QuadStripBuilder quadStripBuilder =
            new QuadStripBuilder(OutVertices, OutIndices, new float3x2(math.up(), new float3(P_UV.x, P_UV.y, 0)));

        float3 currentRay = segStartRay;
        float3x2 quadStripSegment = new float3x2(
            currentRay * radii.x,
            currentRay * radii.y
        );

        quadStripBuilder.Start(quadStripSegment, ref buffersIndexers);
        for (int i = 0; i < P_SegmentResolution; i++)
        {
            currentRay = math.rotate(segQuadRotDelta, currentRay);
            quadStripSegment[0] = (currentRay * radii.x);
            quadStripSegment[1] = (currentRay * radii.y);
            quadStripBuilder.Continue(quadStripSegment, ref buffersIndexers);
        }

        OutBuffersIndexers.ApplyChanges(buffersIndexers);
    }
}