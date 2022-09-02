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
    public NativeArray<QSTransSegment> InputQSTransSegmentBuffer;

    [WriteOnly]
    public QSTransitionsBuffer OutputFadeOutTransitions;

    [WriteOnly]
    public QSTransitionsBuffer OutputFadeInTransitions;

    public void Execute(int i)
    {
        QuadStrip qs = InputQuadStripsCollection.GetQuadStrip(i);
        int2 indexer = InputQuadStripsCollection.GetQuadIndexer(i);

        NativeArray<QSTransSegment> fadeOutWriteBuffer = 
            OutputFadeOutTransitions.GetBufferSegmentAndWriteIndexer(indexer, i);

        NativeArray<QSTransSegment> fadeInWriteBuffer =
            OutputFadeInTransitions.GetBufferSegmentAndWriteIndexer(indexer, i);

        QSTransitionBuilder shuffleTransBuilder = new QSTransitionBuilder();
        
        shuffleTransBuilder.BuildFadeOutTransition(qs, ref fadeOutWriteBuffer);
        shuffleTransBuilder.BuildFadeInTransition(qs, ref fadeInWriteBuffer);
    }
}