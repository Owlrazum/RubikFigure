using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using static Orazum.Math.MathUtilities;

public enum VerticesMoveType
{ 
    Grounded,
    LevitationUp,
    LevitationDown
}

[BurstCompile]
public struct WheelSegmentMoveJob : IJob
{
    public int P_VertexCountInOneSegment;
    public float P_ClockMoveBufferLerpValue; // Assert that it is less than 0.5f;

    public float P_LerpParam;
    public WheelSegmentMesh P_VertexPositions;

    public VerticesMoveType P_moveType;
    public float P_LevitationHeight;

    [ReadOnly]
    public NativeArray<VertexData> InputVertices;

    [WriteOnly]
    public NativeArray<VertexData> OutputVertices;

    public void Execute()
    {
        switch (P_moveType)
        { 
            case VerticesMoveType.Grounded:
                MoveSegmentUpDown();
                break;
            case VerticesMoveType.LevitationDown:
                LevitateSegmentDown();
                break;
            case VerticesMoveType.LevitationUp:
                LevitateSegmentUp();
                break;
        }
    }
    
    private void MoveSegmentUpDown()
    {
        VertexData data;

        for (int i = 0; i < P_VertexPositions.Count; i++)
        {
            int2 indices = P_VertexPositions.GetSegmentIndices(i);
            float3 targetPos = P_VertexPositions.GetPointVertexPos(i);

            data = InputVertices[indices.x];
            data.position = math.lerp(InputVertices[indices.x].position,
                targetPos, P_LerpParam);
            OutputVertices[indices.x] = data;
            if (indices.y < 0)
            {
                continue;
            }

            data = InputVertices[indices.y];
            data.position = math.lerp(InputVertices[indices.y].position,
                targetPos, P_LerpParam);
            OutputVertices[indices.y] = data;
        }
    }

    private void LevitateSegmentDown()
    { 
        VertexData data;

        for (int i = 0; i < P_VertexPositions.Count; i++)
        {
            int2 indices = P_VertexPositions.GetSegmentIndices(i);
            float3 targetPos = P_VertexPositions.GetPointVertexPos(i);

            float innerHeight = 0, outerHeight = 0;
            if (P_LerpParam < 0.5f)
            { 
                float levLerpParam = P_LerpParam * 2;
            }
            else
            {
                float levLerpParam = (P_LerpParam - 0.5f) * 2;
            }

            data = InputVertices[indices.x];

            data.position = math.lerp(InputVertices[indices.x].position,
                targetPos, P_LerpParam);
            // data.position = 

            OutputVertices[indices.x] = data;
            if (indices.y < 0)
            {
                continue;
            }

            data = InputVertices[indices.y];
            data.position = math.lerp(InputVertices[indices.y].position,
                targetPos, P_LerpParam);
            OutputVertices[indices.y] = data;
        }
    }

    private void LevitateSegmentUp()
    { 

    }
}