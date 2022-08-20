using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

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

    private short _totalVertexCount;
    private short _totalIndexCount;

    private short _pointVertexCount;
    private short _pointIndexCount;

    private float3 _startRay;

    private float _startAngle; 
    private float _angleResolutionDelta;

    private int4 _prevCubeStripIndices;

    public void Execute()
    {
        _startAngle = TAU / 4;
        // we subtract so the positive would be clockwiseOrder,
        // with addition it will be counter-clockwise;
        _angleResolutionDelta = TAU / P_SideCount;
        _angleResolutionDelta = _angleResolutionDelta / P_SegmentResolution;

        _startRay = new float3(math.cos(_startAngle), 0, math.sin(_startAngle));

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
        _pointVertexCount = 0;
        _pointIndexCount = 0;

        float3 upDelta = new float3(0, P_Height, 0);
        float3 currentRay = _startRay;
        float3x4 cubeStrip = new float3x4(
            currentRay * radii.x, 
            currentRay * radii.y,
            currentRay * radii.y + upDelta,
            currentRay * radii.x + upDelta
        );
        StartCubeStrip(cubeStrip);
        
        quaternion q = quaternion.AxisAngle(math.up(), _angleResolutionDelta);

        for (int i = 0; i < P_SegmentResolution; i++)
        {
            currentRay =  math.rotate(q, currentRay);
            cubeStrip[0] = currentRay * radii.x;
            cubeStrip[1] = currentRay * radii.y;
            cubeStrip[2] = currentRay * radii.y + upDelta;
            cubeStrip[3] = currentRay * radii.x + upDelta;

            if (i == P_SegmentResolution - 1)
            {
                FinishCubeStrip(cubeStrip);
            }
            else
            { 
                ContinueCubeStrip(cubeStrip);
            }
        }
    }

    private void StartCubeStrip(float3x4 p)
    {
        _prevCubeStripIndices.x = AddVertex(p[0]);
        _prevCubeStripIndices.y = AddVertex(p[1]);
        _prevCubeStripIndices.z = AddVertex(p[2]);
        _prevCubeStripIndices.w = AddVertex(p[3]);
        AddQuadIndices(_prevCubeStripIndices);
    }

    private void ContinueCubeStrip(float3x4 p)
    {
        int4 newCubeStripIndices = int4.zero;
        newCubeStripIndices.x = AddVertex(p[0]);
        newCubeStripIndices.y = AddVertex(p[1]);
        newCubeStripIndices.z = AddVertex(p[2]);
        newCubeStripIndices.w = AddVertex(p[3]);

        int4 quadIndices;
        quadIndices = new int4(newCubeStripIndices.xy, _prevCubeStripIndices.yx);
        AddQuadIndices(quadIndices);
        quadIndices = new int4(newCubeStripIndices.yz, _prevCubeStripIndices.zy);
        AddQuadIndices(quadIndices);
        quadIndices = new int4(newCubeStripIndices.zw, _prevCubeStripIndices.wz);
        AddQuadIndices(quadIndices);
        quadIndices = new int4(_prevCubeStripIndices.xw, newCubeStripIndices.wx);
        AddQuadIndices(quadIndices);

        _prevCubeStripIndices = newCubeStripIndices;
    }

    private void FinishCubeStrip(float3x4 p)
    { 
        int4 newCubeStripIndices = int4.zero;
        newCubeStripIndices.x = AddVertex(p[0]);
        newCubeStripIndices.y = AddVertex(p[1]);
        newCubeStripIndices.z = AddVertex(p[2]);
        newCubeStripIndices.w = AddVertex(p[3]);

        int4 quadIndices;
        quadIndices = new int4(newCubeStripIndices.xy, _prevCubeStripIndices.yx);
        AddQuadIndices(quadIndices);
        quadIndices = new int4(newCubeStripIndices.yz, _prevCubeStripIndices.zy);
        AddQuadIndices(quadIndices);
        quadIndices = new int4(newCubeStripIndices.zw, _prevCubeStripIndices.wz);
        AddQuadIndices(quadIndices);
        quadIndices = new int4(_prevCubeStripIndices.xw, newCubeStripIndices.wx);
        AddQuadIndices(quadIndices);
        quadIndices = new int4(newCubeStripIndices.wzyx);
        AddQuadIndices(quadIndices);

        _prevCubeStripIndices = newCubeStripIndices;
    }

    private void AddQuadIndices(int4 quadIndices)
    {
        AddIndex(quadIndices.x);
        AddIndex(quadIndices.y);
        AddIndex(quadIndices.z);
        AddIndex(quadIndices.x);
        AddIndex(quadIndices.z);
        AddIndex(quadIndices.w);
    }

    private short AddVertex(float3 pos)
    { 
        OutputVertices[_totalVertexCount++] = pos;
        short addedVertexIndex = _pointVertexCount;
        _pointVertexCount++;

        return addedVertexIndex;
    }

    private void AddIndex(int vertexIndex)
    {
        OutputIndices[_totalIndexCount++] = (short)vertexIndex;
    }
}