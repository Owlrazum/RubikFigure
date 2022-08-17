using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using static Orazum.Math.MathUtilities;

using UnityEngine;

[BurstCompile]
public struct WheelGenJob : IJob
{
    public int P_SideCount;
    public int P_RingCount;
    public int P_SegmentResolution;
    
    public float P_InnerCircleRadius;
    public float P_OuterCircleRadius;

    [WriteOnly]
    public NativeArray<VertexData> OutputVertices;

    [WriteOnly]
    public NativeArray<short> OutputIndices;

    [WriteOnly]
    public NativeArray<SegmentMesh> OutputSegmentMeshes; // one for each ring

    private short _totalVertexCount;
    private short _totalIndexCount;

    private short _segmentVertexCount;
    private short _segmentIndexCount;

    private float2 _uv;
    private int _segmentIndex;
    private float _currentRadius;
    private float _nextRadius;

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
        float lerpDeltaToResolution = 1 / P_SegmentResolution;
        bool isInitializedSegmentVertexPositions = false;
        float4 positionsData = new float4(-1, -1,
            TAU / P_SideCount, 1 / lerpDeltaToResolution);

        for (int side = 0; side < P_SideCount; side++)
        {
            float deltaUV = 1.0f / P_SideCount;
            float startUV = 1 - deltaUV / 2;
            _uv = new float2(0, startUV - side * deltaUV);

            _currentRadius = P_InnerCircleRadius;
            _nextRadius = _currentRadius + radiusDelta;


            for (int ring = 0; ring < P_RingCount; ring++)
            {
                _segmentIndex = side * P_RingCount + ring;

                AddSegment();

                if (!isInitializedSegmentVertexPositions)
                {
                    positionsData.x = _currentRadius;
                    positionsData.y = _nextRadius;
                    positionsData.z = _angleResolutionDelta;
                    // positionsData.w = 1.0f / (P_SegmentResolution);

                    OutputSegmentMeshes[ring] = new SegmentMesh(_startRay, positionsData, P_SegmentResolution);
                } 
                _currentRadius = _nextRadius;
                _nextRadius += radiusDelta;
            }
            if (!isInitializedSegmentVertexPositions)
            {
                isInitializedSegmentVertexPositions = true;
            }
        }
    }

    private void AddSegment()
    {
        _segmentVertexCount = 0;
        _segmentIndexCount = 0;

        float3 currentRay = _startRay;
        quaternion q = quaternion.AxisAngle(math.up(), _angleResolutionDelta);
        float3 nextRay =  math.rotate(q, currentRay);

        for (int i = 0; i < P_SegmentResolution; i++)
        {
            float3x4 quadPoses = new float3x4(
                currentRay * _currentRadius,
                currentRay * _nextRadius,
                nextRay * _nextRadius,
                nextRay * _currentRadius
            );

            AddQuad(quadPoses, math.up());

            currentRay = nextRay;
            nextRay = math.rotate(q, nextRay);
        }
    }

    private void AddQuad(float3x4 positions, float3 normal)
    { 
        short diagonal_1 = AddVertex(positions[0], normal);
        AddVertex(positions[1], normal);
        short diagonal_2 = AddVertex(positions[2], normal);

        AddIndex(diagonal_1);
        AddIndex(diagonal_2);
        AddVertex(positions[3], normal);
    }

    private short AddVertex(float3 pos, float3 normal)
    { 
        VertexData vertex = new VertexData();
        vertex.position = pos;
        vertex.normal = normal;
        vertex.uv = _uv;
        
        OutputVertices[_totalVertexCount++] = vertex;
        short addedVertexIndex = _segmentVertexCount;
        _segmentVertexCount++;

        OutputIndices[_totalIndexCount++] = addedVertexIndex;
        _segmentIndexCount++;

        return addedVertexIndex;
    }

    private void AddIndex(short vertexIndex)
    {
        _segmentIndexCount++;
        OutputIndices[_totalIndexCount++] = vertexIndex;
    }
}