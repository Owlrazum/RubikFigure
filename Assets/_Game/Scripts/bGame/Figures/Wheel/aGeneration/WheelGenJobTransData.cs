using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

// [BurstCompile]
public struct WheelGenJobTransData : IJobFor
{
    [ReadOnly]
    public QuadStripsBuffer InQuadStripsCollection;

    [ReadOnly]
    public SegmentedBufferInt2 InOriginsTargetsIndices;

    public QSTransitionsBuffer OutTransitionsCollection;
    
    public void Execute(int i)
    {
        // int2 originTarget = InOriginTargetIndices[i];

        // int2 bufferIndexer = OutTransitionsCollection.GetIndexer(i);
        // NativeArray<QSTransSegment> writeBuffer = 
        //     OutTransitionsCollection.GetBufferSegmentAndWriteIndexer(bufferIndexer, i);

        // QuadStrip origin = InQuadStripsCollection.GetQuadStrip(originTarget.x);
        // QuadStrip target = InQuadStripsCollection.GetQuadStrip(originTarget.y);

        // WheelTransitionsBuilder transDataBuilder = new WheelTransitionsBuilder();
        // transDataBuilder.BuildTransition(ref writeBuffer);
    }
}