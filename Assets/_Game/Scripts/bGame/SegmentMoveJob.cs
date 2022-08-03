using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

public enum SegmentMoveType
{ 
    Up,
    Down,
    Clockwise,
    CounterClockwise
}

[BurstCompile]
public struct SegmentMoveJob : IJob
{
    public int P_VertexCountInOneSegment;
    public float P_ClockMoveBufferLerpValue; // Assert that it is less than 0.5f;

    public float P_LerpParam;
    public SegmentMoveType P_SegmentMoveType;
    public SegmentPoint P_SegmentPoint;

    [ReadOnly]
    public NativeArray<VertexData> InputVertices;

    [WriteOnly]
    public NativeArray<VertexData> OutputVertices;

    public void Execute()
    {
        switch (P_SegmentMoveType)
        { 
            case SegmentMoveType.Up:
            case SegmentMoveType.Down:
                MoveSegmentUpDown();
                break;
            case SegmentMoveType.Clockwise:
            case SegmentMoveType.CounterClockwise:
                MoveSegmentClockOrder();
                break;
        }
    }

    private void MoveSegmentUpDown()
    {
        VertexData data;
        for (int corner = 0; corner < 8; corner++)
        {
            int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner);
            float3 cornerPos = P_SegmentPoint.GetCornerPosition(corner);

            data = InputVertices[meshCorner.x];
            data.position = math.lerp(InputVertices[meshCorner.x].position,
                cornerPos, P_LerpParam);
            OutputVertices[meshCorner.x] = data;

            data = InputVertices[meshCorner.y];
            data.position = math.lerp(InputVertices[meshCorner.y].position,
               cornerPos, P_LerpParam);
            OutputVertices[meshCorner.y] = data;

            data = InputVertices[meshCorner.z];
            data.position = math.lerp(InputVertices[meshCorner.z].position,
               cornerPos, P_LerpParam);
            OutputVertices[meshCorner.z] = data;
        }
    }

    private void MoveSegmentClockOrder()
    {
        VertexData data;

        if (P_LerpParam < P_ClockMoveBufferLerpValue)
        {
            P_LerpParam = math.unlerp(0, 0.5f, P_LerpParam);
            for (int corner = 0; corner < 8; corner++)
            {
                int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner);
                float3 cornerPos = P_SegmentPoint.GetCornerPosition(corner);

                if (corner < 4)
                { 
                    #region ApproachSegmentPoint
                    data = InputVertices[meshCorner.x];
                    data.position = math.lerp(InputVertices[meshCorner.x].position,
                        cornerPos, P_LerpParam);
                    OutputVertices[meshCorner.x] = data;

                    data = InputVertices[meshCorner.y];
                    data.position = math.lerp(InputVertices[meshCorner.y].position,
                        cornerPos, P_LerpParam);
                    OutputVertices[meshCorner.y] = data;

                    data = InputVertices[meshCorner.z];
                    data.position = math.lerp(InputVertices[meshCorner.z].position,
                        cornerPos, P_LerpParam);
                    OutputVertices[meshCorner.z] = data;
                    #endregion
                }
                else
                {
                    OutputVertices[meshCorner.x] = InputVertices[meshCorner.x];
                    OutputVertices[meshCorner.y] = InputVertices[meshCorner.y];
                    OutputVertices[meshCorner.z] = InputVertices[meshCorner.z];
                }
            }
        }
        else if (P_LerpParam < 0.5f)
        {
            float P_LerpParamOrigin = P_LerpParam;
            P_LerpParam = math.unlerp(0, 0.5f, P_LerpParamOrigin);

            for (int corner = 0; corner < 4; corner++)
            {
                #region ApproachSegmentPoint
                int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner);
                float3 cornerPos = P_SegmentPoint.GetCornerPosition(corner);

                data = InputVertices[meshCorner.x];
                data.position = math.lerp(InputVertices[meshCorner.x].position,
                   cornerPos, P_LerpParam);
                OutputVertices[meshCorner.x] = data;

                data = InputVertices[meshCorner.y];
                data.position = math.lerp(InputVertices[meshCorner.y].position,
                   cornerPos, P_LerpParam);
                OutputVertices[meshCorner.y] = data;

                data = InputVertices[meshCorner.z];
                data.position = math.lerp(InputVertices[meshCorner.z].position,
                   cornerPos, P_LerpParam);
                OutputVertices[meshCorner.z] = data;
                #endregion
            }

            P_LerpParam = math.unlerp(P_ClockMoveBufferLerpValue, 1, P_LerpParamOrigin);
            for (int corner = 4; corner < 8; corner++)
            {
                #region ApproachSegmentPoint
                int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner);
                float3 cornerPos = P_SegmentPoint.GetCornerPosition(corner);

                data = InputVertices[meshCorner.x];
                data.position = math.lerp(InputVertices[meshCorner.x].position,
                   cornerPos, P_LerpParam);
                OutputVertices[meshCorner.x] = data;

                data = InputVertices[meshCorner.y];
                data.position = math.lerp(InputVertices[meshCorner.y].position,
                   cornerPos, P_LerpParam);
                OutputVertices[meshCorner.y] = data;

                data = InputVertices[meshCorner.z];
                data.position = math.lerp(InputVertices[meshCorner.z].position,
                   cornerPos, P_LerpParam);
                OutputVertices[meshCorner.z] = data;
                #endregion
            }
        }
        else
        {
            P_LerpParam = math.unlerp(P_ClockMoveBufferLerpValue, 1, P_LerpParam);
            for (int corner = 0; corner < 8; corner++)
            {
                int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner);
                float3 cornerPos = P_SegmentPoint.GetCornerPosition(corner);

                if (corner < 4)
                {
                    data = InputVertices[meshCorner.x];
                    data.position = cornerPos;
                    OutputVertices[meshCorner.x] = data;

                    data = InputVertices[meshCorner.y];
                    data.position = cornerPos;
                    OutputVertices[meshCorner.y] = data;

                    data = InputVertices[meshCorner.z];
                    data.position = cornerPos;
                    OutputVertices[meshCorner.z] = data;
                }
                else
                { 
                    #region ApproachSegmentPoint
                    data = InputVertices[meshCorner.x];
                    data.position = math.lerp(InputVertices[meshCorner.x].position,
                        cornerPos, P_LerpParam);
                    OutputVertices[meshCorner.x] = data;

                    data = InputVertices[meshCorner.y];
                    data.position = math.lerp(InputVertices[meshCorner.y].position,
                        cornerPos, P_LerpParam);
                    OutputVertices[meshCorner.y] = data;

                    data = InputVertices[meshCorner.z];
                    data.position = math.lerp(InputVertices[meshCorner.z].position,
                        cornerPos, P_LerpParam);
                    OutputVertices[meshCorner.z] = data;
                    #endregion
                }
            }
        }
    }
}