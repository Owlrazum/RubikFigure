using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Math;

// [BurstCompile]
public struct WheelGenJobTransData : IJobFor
{
    public int P_VertOrderTransitionsCount;
    public int P_SideCount;
    public int P_RingCount;
    public int P_SegmentResolution;

    [ReadOnly]
    public QuadStripsBuffer InQuadStripsCollection;

    [ReadOnly]
    public SegmentedBufferInt2 InOriginsTargetsIndicesBuffer;

    public QS_TransitionsBuffer OutTransitionsBuffer;
    
    public void Execute(int i)
    {
        NativeArray<int2> originTargets = InOriginsTargetsIndicesBuffer.GetBufferSegment(i);
        NativeArray<QST_Segment> writeBuffer = OutTransitionsBuffer.GetBufferSegment(i);

        WheelTransitionsBuilder transDataBuilder = new WheelTransitionsBuilder(
            P_SideCount,
            P_SegmentResolution
        );
        if (i < P_VertOrderTransitionsCount)
        {
            VertOrderType vertOrder = VertOrderType.Up;
            if (i % 2 == 1)
            {
                vertOrder = VertOrderType.Down;
            }
            transDataBuilder.BuildVertOrderTransition(
                ref InQuadStripsCollection,
                ref originTargets,
                ref writeBuffer,
                vertOrder
            );
        }
        else
        { 
            ClockOrderType clockOrder = ClockOrderType.CW;
            if (i % 2 == 1)
            {
                clockOrder = ClockOrderType.AntiCW;
            }
            transDataBuilder.BuildClockOrderTransition(
                ref InQuadStripsCollection,
                ref originTargets,
                ref writeBuffer,
                clockOrder
            );
        }
    }
}