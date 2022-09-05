using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Meshing;

[BurstCompile]
public struct FigureGenJobShuffleTrans : IJobFor
{
    [ReadOnly]
    public QuadStripsBuffer InputQuadStripsCollection;

    [WriteOnly]
    public NativeArray<QST_Segment> InputQSTransSegmentBuffer;

    [WriteOnly]
    public QS_TransitionsBuffer OutputFadeOutTransitions;

    [WriteOnly]
    public QS_TransitionsBuffer OutputFadeInTransitions;

    public void Execute(int i)
    {
        QuadStrip qs = InputQuadStripsCollection.GetQuadStrip(i);
        int2 indexer = InputQuadStripsCollection.GetQuadIndexer(i);

        NativeArray<QST_Segment> fadeOutWriteBuffer = 
            OutputFadeOutTransitions.GetBufferSegmentAndWriteIndexer(indexer, i);

        NativeArray<QST_Segment> fadeInWriteBuffer =
            OutputFadeInTransitions.GetBufferSegmentAndWriteIndexer(indexer, i);

        QSTS_QuadBuilder shuffleTransBuilder = new QSTS_QuadBuilder();
        
        shuffleTransBuilder.BuildFadeOutTransition(qs, ref fadeOutWriteBuffer);
        shuffleTransBuilder.BuildFadeInTransition(qs, ref fadeInWriteBuffer);
    }
}