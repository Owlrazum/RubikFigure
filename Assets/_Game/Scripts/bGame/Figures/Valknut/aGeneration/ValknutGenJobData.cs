using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Meshing;
using Orazum.Math;
using static Orazum.Math.ClockOrderConversions;

// [BurstCompile]
public struct ValknutGenJobData : IJobFor
{
    [ReadOnly]
    public NativeArray<ValknutSegmentMesh> InputSegmentMeshes;

    [ReadOnly]
    public NativeArray<int3x2> InputIndexData;

    [WriteOnly]
    public NativeArray<float4x2> OutputTransitionPositions;
    
    [WriteOnly]
    public NativeArray<float3> OutputLerpRanges;


    public void Execute(int i)
    {
        int3x2 index = InputIndexData[i];
        NativeArray<float4x2> transitionPositionsSlice = 
            OutputTransitionPositions.GetSubArray(index[1].x, index[1].y);
        NativeArray<float3> lerpRangesSlice =
            OutputLerpRanges.GetSubArray(index[1].x, index[1].y);
            
        ValknutUtilities.BuildTransitionData(
            InputSegmentMeshes[index[0].x],
            InputSegmentMeshes[index[0].y],
            IntToClockOrder(index[0].z),
            ref transitionPositionsSlice,
            ref lerpRangesSlice
        );
    }
}