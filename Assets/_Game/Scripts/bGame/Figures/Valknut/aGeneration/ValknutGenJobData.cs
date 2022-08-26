using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

// [BurstCompile]
public struct ValknutGenJobData : IJobFor
{
    [ReadOnly]
    public QuadStripsCollection InputQuadStripsCollection;

    [ReadOnly]
    public NativeArray<int2x2> InputIndexData;

    [WriteOnly]
    public NativeArray<QSTransSegment> OutputTransitionsSegments;
    
    public void Execute(int i)
    {
        int2x2 index = InputIndexData[i];
        NativeArray<QSTransSegment> transitionSegments = 
            OutputTransitionsSegments.GetSubArray(index[1].x, index[1].y);

        QuadStrip tempOrigin = InputQuadStripsCollection.GetTempQuadStrip(index[0].x);
        QuadStrip tempTarget = InputQuadStripsCollection.GetTempQuadStrip(index[0].y);

        ValknutTransitionsBuilder dataBuilder = new ValknutTransitionsBuilder(
            tempOrigin,
            tempTarget,
            ref transitionSegments
        );

        dataBuilder.BuildTransitionData();

        tempOrigin.Dispose();
        tempTarget.Dispose();
    }
}