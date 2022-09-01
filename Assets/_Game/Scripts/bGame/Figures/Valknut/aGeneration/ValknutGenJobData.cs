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
    public NativeArray<int2> InputOriginTargetIndices;

    public QSTransitionsCollection OutputTransitionsCollection;
    
    public void Execute(int i)
    {
        int2 originTarget = InputOriginTargetIndices[i];

        int2 bufferIndexer = OutputTransitionsCollection.GetIndexer(i);
        NativeArray<QSTransSegment> writeBuffer = 
            OutputTransitionsCollection.GetWriteBufferAndWriteIndexer(bufferIndexer, i);

        QuadStrip origin = InputQuadStripsCollection.GetQuadStrip(originTarget.x);
        QuadStrip target = InputQuadStripsCollection.GetQuadStrip(originTarget.y);

        ValknutTransitionsBuilder dataBuilder = new ValknutTransitionsBuilder(
            origin,
            target
        );

        dataBuilder.BuildTransition(ref writeBuffer);
    }
}