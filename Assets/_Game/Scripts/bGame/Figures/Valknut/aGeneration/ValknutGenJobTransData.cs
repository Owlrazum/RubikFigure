using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

// [BurstCompile]
public struct ValknutGenJobTransData : IJobFor
{
    [ReadOnly]
    public QuadStripsBuffer InQuadStripsCollection;

    [ReadOnly]
    public NativeArray<int2> InOriginTargetIndices;

    public QSTransitionsBuffer OutTransitionsCollection;
    
    public void Execute(int i)
    {
        int2 originTarget = InOriginTargetIndices[i];
        QuadStrip origin = InQuadStripsCollection.GetQuadStrip(originTarget.x);
        QuadStrip target = InQuadStripsCollection.GetQuadStrip(originTarget.y);

        ValknutTransitionsBuilder dataBuilder = new ValknutTransitionsBuilder(
            origin,
            target
        );

        NativeArray<QSTransSegment> writeBuffer = 
            OutTransitionsCollection.GetBufferSegment(i);

        dataBuilder.BuildTransition(ref writeBuffer);
    }
}