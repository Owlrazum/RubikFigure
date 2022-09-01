using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using Orazum.Meshing;

[BurstCompile]
public struct FigureGenJobShuffleTrans : IJobFor
{
    [ReadOnly]
    public QuadStripsCollection InputQuadStripsCollection;

    [WriteOnly]
    public NativeArray<QSTransSegment> InputQSTransSegmentBuffer;

    [WriteOnly]
    public QSTransitionsCollection OutputFadeOutTransitions;

    [WriteOnly]
    public QSTransitionsCollection OutputFadeInTransitions;

    public void Execute(int i)
    {
        QuadStrip qs = InputQuadStripsCollection.GetQuadStrip(i);
        int2 indexer = InputQuadStripsCollection.GetQuadIndexer(i);

        NativeArray<QSTransSegment> fadeOutWriteBuffer = 
            OutputFadeOutTransitions.GetWriteBufferAndWriteIndexer(indexer, i);

        NativeArray<QSTransSegment> fadeInWriteBuffer =
            OutputFadeInTransitions.GetWriteBufferAndWriteIndexer(indexer, i);

        QSTransitionBuilder shuffleTransBuilder = new QSTransitionBuilder();
        
        shuffleTransBuilder.BuildFadeOutTransition(qs, ref fadeOutWriteBuffer);
        shuffleTransBuilder.BuildFadeInTransition(qs, ref fadeInWriteBuffer);
    }
}