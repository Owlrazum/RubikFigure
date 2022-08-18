using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

[BurstCompile]
public struct WheelSegmentMoveJob : IJob
{
    public int P_VertexCountInOneSegment;
    public float P_ClockMoveBufferLerpValue; // Assert that it is less than 0.5f;

    public float P_LerpParam;
    public WheelSegmentMesh P_VertexPositions;

    [ReadOnly]
    public NativeArray<VertexData> InputVertices;

    [WriteOnly]
    public NativeArray<VertexData> OutputVertices;

    public void Execute()
    {
        MoveSegmentUpDown();
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
}