using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using static Orazum.Math.MathUtilities;

// [BurstCompile]
public struct SegmentPointMeshGenJob : IJob
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
            AddSegment(radiuses);

            radiuses.x = radiuses.y;
            radiuses.y += radiusDelta;
        }
    }

    private void AddSegment(float2 radii)
    {
        _pointVertexCount = 0;
        _pointIndexCount = 0;
        
        quaternion q = quaternion.AxisAngle(math.up(), _angleResolutionDelta);
        float3x2 rays = new float3x2(
            _startRay,
            math.rotate(q, _startRay)
        );

        float3 h = math.up() * P_Height;

        float3x4 leftQuad = new float3x4(
            rays[0] * radii.y,
            rays[0] * radii.y + h,
            rays[0] * radii.x + h,
            rays[0] * radii.x
        );
        AddQuad(leftQuad);

        for (int i = 0; i < P_SegmentResolution; i++)
        {
            float3x4 botQuad = new float3x4(
                rays[1] * radii.x,
                rays[1] * radii.y,
                rays[0] * radii.y,
                rays[0] * radii.x
            );
            AddQuad(botQuad);

            float3x4 topQuad = new float3x4(
                rays[0] * radii.x + h,
                rays[0] * radii.y + h,
                rays[1] * radii.y + h,
                rays[1] * radii.x + h
            );
            AddQuad(topQuad);

            float3x4 backQuad = new float3x4(
                rays[0] * radii.x,
                rays[0] * radii.x + h,
                rays[1] * radii.x + h,
                rays[1] * radii.x
            );
            AddQuad(backQuad);

            float3x4 forwQuad = new float3x4(
                rays[1] * radii.y,
                rays[1] * radii.y + h,
                rays[0] * radii.y + h,
                rays[0] * radii.y
            );
            AddQuad(forwQuad);
            
            UnityEngine.Debug.DrawRay(UnityEngine.Vector3.zero, rays[0] * 100, UnityEngine.Color.red, 100);
            UnityEngine.Debug.DrawRay(UnityEngine.Vector3.zero, rays[1] * 100, UnityEngine.Color.red, 100);
            rays[0] = rays[1];
            rays[1] = math.rotate(q, rays[1]);
        }

        float3x4 rightQuad = new float3x4(
            rays[0] * radii.x,
            rays[0] * radii.x + h,
            rays[0] * radii.y + h,
            rays[0] * radii.y
        );
        AddQuad(rightQuad);
    }

    private void AddQuad(float3x4 positions)
    { 
        short diagonal_1 = AddVertex(positions[0]);
        AddVertex(positions[1]);
        short diagonal_2 = AddVertex(positions[2]);

        AddIndex(diagonal_1);
        AddIndex(diagonal_2);
        AddVertex(positions[3]);
    }

    private short AddVertex(float3 pos)
    { 
        OutputVertices[_totalVertexCount++] = pos;
        short addedVertexIndex = _pointVertexCount++;

        OutputIndices[_totalIndexCount++] = addedVertexIndex;
        _pointIndexCount++;

        return addedVertexIndex;
    }

    private void AddIndex(short vertexIndex)
    {
        OutputIndices[_totalIndexCount++] = vertexIndex;
        _pointIndexCount++;
    }
}