using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using Orazum.Meshing;

// [BurstCompile]
public struct FigureGenJobShuffleTrans : IJobFor
{
    [ReadOnly]
    public QuadStripsBuffer InputQuadStripsCollection;

    [WriteOnly]
    public QS_TransitionsBuffer OutputQSTransSegmentBuffer;

    public void Execute(int i)
    {
        QuadStrip qs = InputQuadStripsCollection.GetQuadStrip(i / 2);
        int2 indexer = InputQuadStripsCollection.GetQuadIndexer(i / 2);

        QSTS_QuadBuilder shuffleTransBuilder = new QSTS_QuadBuilder();

        if (i % 2 == 0)
        { 
            NativeArray<QST_Segment> fadeOutWriteBuffer = 
                OutputQSTransSegmentBuffer.GetBufferSegmentAndWriteIndexer(indexer, i);
            shuffleTransBuilder.BuildFadeOutTransition(qs, ref fadeOutWriteBuffer);
        }
        else
        { 
            NativeArray<QST_Segment> fadeInWriteBuffer =
                OutputQSTransSegmentBuffer.GetBufferSegmentAndWriteIndexer(indexer, i);
            shuffleTransBuilder.BuildFadeInTransition(qs, ref fadeInWriteBuffer);
        }
    }
}