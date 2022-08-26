using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Meshing;
using static Orazum.Math.MathUtilities;

[BurstCompile]
public struct WheelSegmentPointMeshGenJob : IJob
{
    public int P_SideCount;
    public int P_RingCount;
    public int P_SegmentResolution;
    
    public float P_InnerCircleRadius;
    public float P_OuterCircleRadius;
    public float P_Height;
    
    [WriteOnly]
    public NativeArray<float3> OutputVertices;

    [WriteOnly]
    public NativeArray<short> OutputIndices;

    private MeshBuffersIndexers _buffersData;

    private float3 _startRay;
    private quaternion _rotationDelta;

    public void Execute()
    {
        _buffersData = new MeshBuffersIndexers();

        _startRay = new float3(math.cos(TAU / 4), 0, math.sin(TAU / 4));
        float angleResolutionDelta = TAU / P_SideCount;
        angleResolutionDelta = angleResolutionDelta / P_SegmentResolution;
        _rotationDelta = quaternion.AxisAngle(math.up(), angleResolutionDelta);

        float radiusDelta = (P_OuterCircleRadius - P_InnerCircleRadius) / P_RingCount;
        float2 radiuses = new float2(P_InnerCircleRadius, P_InnerCircleRadius + radiusDelta);

        for (int ring = 0; ring < P_RingCount; ring++)
        {
            AddSegmentPoint(radiuses);

            radiuses.x = radiuses.y;
            radiuses.y += radiusDelta;
        }
    }

    private void AddSegmentPoint(float2 radii)
    {
        _buffersData.LocalCount = int2.zero;
        CubeStrip cubeStrip = new CubeStrip(OutputVertices, OutputIndices);

        float3 upDelta = new float3(0, P_Height, 0);
        float3 currentRay = _startRay;
        float3x4 cubeStripSegment = new float3x4(
            currentRay * radii.x, 
            currentRay * radii.y,
            currentRay * radii.y + upDelta,
            currentRay * radii.x + upDelta
        );
        cubeStrip.Start(cubeStripSegment, ref _buffersData);
        
        for (int i = 0; i < P_SegmentResolution; i++)
        {
            currentRay =  math.rotate(_rotationDelta, currentRay);
            cubeStripSegment[0] = currentRay * radii.x;
            cubeStripSegment[1] = currentRay * radii.y;
            cubeStripSegment[2] = currentRay * radii.y + upDelta;
            cubeStripSegment[3] = currentRay * radii.x + upDelta;

            if (i == P_SegmentResolution - 1)
            {
                cubeStrip.Finish(cubeStripSegment, ref _buffersData);
            }
            else
            { 
                cubeStrip.Continue(cubeStripSegment, ref _buffersData);
            }
        }
    }
}