using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

[BurstCompile]
public struct WheelGenJob : IJob
{
    public float P_WheelHeight;
    public float P_OuterCircleRadius;
    public float P_InnerCircleRadius;
    public int P_SideCount;
    public int P_SegmentsCount;

    // The size of array is an amount of segments
    public NativeArray<short> OutputVertexCounts;

    // The size of array is an amount of segments
    public NativeArray<short> OutputIndexCounts;

    [WriteOnly]
    public NativeArray<VertexData> OutputVertices;

    [WriteOnly]
    public NativeArray<short> OutputIndices;

    private short totalVertexCount;
    private short totalIndexCount;

    private float2 _uv;
    private int _segmentIndex;
    private float _currentRadius;
    private float _nextRadius;

    public void Execute()
    {
        Assert.IsTrue(P_SideCount >= 3 && P_SideCount <= 6);

        CircleRays rays = new CircleRays();
#region RaysInit
        float angleDelta = 2 * math.PI / P_SideCount;
        float currentAngle = math.PI / 2;
        rays[0] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
        currentAngle += angleDelta;
        rays[1] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
        currentAngle += angleDelta;
        rays[2] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
        currentAngle += angleDelta;

        if (P_SideCount >= 4)
        { 
            rays[3] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
            currentAngle += angleDelta;
        }
        if (P_SideCount >= 5)
        { 
            rays[4] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
            currentAngle += angleDelta;
        }
        if (P_SideCount >= 6)
        { 
            rays[5] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
            currentAngle += angleDelta;
        }
#endregion

        float radiusDelta = (P_OuterCircleRadius - P_InnerCircleRadius) / P_SegmentsCount;

        for (int i = 0; i < P_SideCount; i++)
        {
            int currentRayIndex = i;
            int nextRayIndex = i + 1 >= P_SideCount ? 0 : i + 1;

            _uv = new float2(0.25f, 0.25f);
            if (P_SideCount >= 3)
            {
                _uv += new float2(0.5f, 0);
            }
            _uv += new float2(0, i * 1.0f / 3);

            _currentRadius = P_InnerCircleRadius;
            _nextRadius = _currentRadius + radiusDelta;

            
            for (int j = 0; j < P_SegmentsCount; j++)
            {
                _segmentIndex = i * P_SegmentsCount + j;

                AddSegment(rays[currentRayIndex], rays[nextRayIndex]);

                _currentRadius = _nextRadius;
                _nextRadius += radiusDelta;
            }
        }
    }

    private void AddSegment(float3 currentRay, float3 nextRay)
    {
        OutputVertexCounts[_segmentIndex] = 0;
        OutputIndexCounts[_segmentIndex] = 0;

        float3x4 posBot = new float3x4(
            currentRay * _currentRadius,
            currentRay * _nextRadius,
            nextRay * _currentRadius,
            nextRay * _nextRadius
        );

        float3x4 posQuadBot = new float3x4(
            posBot[2],
            posBot[3],
            posBot[1],
            posBot[0]
        );

        AddQuad(posQuadBot, math.down());
        
        float3x4 posTop = new float3x4(
            posBot[0] + math.up() * P_WheelHeight,
            posBot[1] + math.up() * P_WheelHeight,
            posBot[2] + math.up() * P_WheelHeight,
            posBot[3] + math.up() * P_WheelHeight
        );

        float3x4 posQuadTop = new float3x4(
            posTop[0],
            posTop[1],
            posTop[3],
            posTop[2]
        );
        AddQuad(posQuadTop, math.up());

        float3x4 posLeft = new float3x4(
            posBot[1],
            posTop[1],
            posTop[0],
            posBot[0]
        );
        float3 left = math.rotate(quaternion.AxisAngle(math.up(), -90), currentRay);;
        AddQuad(posLeft, left);

        float3x4 posForward = new float3x4(
            posBot[3],
            posTop[3],
            posTop[1],
            posBot[1]
        );
        float3 forward = (currentRay + nextRay) / 2;
        AddQuad(posForward, forward);

        float3x4 posRight = new float3x4(
            posBot[2],
            posTop[2],
            posTop[3],
            posBot[3]
        );
        float3 right = math.rotate(quaternion.AxisAngle(math.up(), 90), nextRay);;
        AddQuad(posRight, right);

        float3x4 posBack = new float3x4(
            posBot[0],
            posTop[0],
            posTop[2],
            posBot[2]
        );
        float3 back = -forward;
        AddQuad(posBack, back);
    }

    private void AddQuad(float3x4 positions, float3 normal)
    { 
        AddVertex(positions[1], normal);
        short diagonal_1 = AddVertex(positions[0], normal);
        short diagonal_2 = AddVertex(positions[2], normal);

        AddIndex(diagonal_2);
        AddIndex(diagonal_1);
        AddVertex(positions[3], normal);
    }

    private short AddVertex(float3 pos, float3 normal)
    { 
        VertexData vertex = new VertexData();
        vertex.position = pos;
        vertex.normal = normal;
        vertex.uv = _uv;
        
        OutputVertices[totalVertexCount++] = vertex;
        short addedVertexIndex = OutputVertexCounts[_segmentIndex];
        OutputVertexCounts[_segmentIndex]++;

        OutputIndices[totalIndexCount++] = addedVertexIndex;
        OutputIndexCounts[_segmentIndex]++;

        return addedVertexIndex;
    }

    private void AddIndex(short vertexIndex)
    {
        OutputIndexCounts[_segmentIndex]++;
        OutputIndices[totalIndexCount++] = vertexIndex;
    }
}