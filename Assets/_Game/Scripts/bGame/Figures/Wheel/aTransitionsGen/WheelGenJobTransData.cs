using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using static Orazum.Collections.IndexUtilities;

using Orazum.Meshing;

// [BurstCompile]
public struct WheelGenJobTransData : IJobFor
{
    public int P_VertOrderDoubleTransitionsCount;
    public int2 P_SidesRingsCount;
    public int P_SegmentResolution;

    [ReadOnly]
    public QuadStripsBuffer InQuadStripsCollection;

    public QS_TransitionsBuffer OutTransitionsBuffer;
    
    public void Execute(int doubleTransitionIndex)
    {
        NativeArray<QST_Segment> writeBuffer = OutTransitionsBuffer.GetBufferSegment(doubleTransitionIndex);

        WheelTransitionsBuilder transDataBuilder = new WheelTransitionsBuilder(
            P_SidesRingsCount,
            P_SegmentResolution
        );

        if (doubleTransitionIndex < P_VertOrderDoubleTransitionsCount)
        {
            transDataBuilder.BuildVertOrderTransition(
                ref InQuadStripsCollection,
                writeBuffer.GetSubArray(0, writeBuffer.Length / 2),
                writeBuffer.GetSubArray(writeBuffer.Length / 2, writeBuffer.Length / 2),
                doubleTransitionIndex
            );
        }
        else
        {
            transDataBuilder.BuildClockOrderTransition(
                ref InQuadStripsCollection,
                writeBuffer.GetSubArray(0, writeBuffer.Length / 2),
                writeBuffer.GetSubArray(writeBuffer.Length / 2, writeBuffer.Length / 2),
                doubleTransitionIndex - P_VertOrderDoubleTransitionsCount
            );
        }
    }
}