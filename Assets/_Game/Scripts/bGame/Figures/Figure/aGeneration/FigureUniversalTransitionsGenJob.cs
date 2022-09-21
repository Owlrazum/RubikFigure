using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using Orazum.Meshing;

// [BurstCompile]
public struct FigureUniversalTransitionsGenJob : IJobFor
{
    [ReadOnly]
    public QuadStripsBuffer InputQuadStripsCollection;

    [ReadOnly]
    public NativeArray<int2> InputQSTransitionsBufferIndexers;

    [WriteOnly]
    public QS_TransitionsBuffer OutputQSTransitionsBuffer;

    public void Execute(int i)
    {
        QuadStrip qs = InputQuadStripsCollection.GetQuadStrip(i / 2);
        int2 indexer = InputQSTransitionsBufferIndexers[i];
        QSTS_QuadBuilder shuffleTransBuilder = new QSTS_QuadBuilder();

        if (i % 2 == 0)
        { 
            NativeArray<QST_Segment> fadeOutWriteBuffer = 
                OutputQSTransitionsBuffer.GetBufferSegmentAndWriteIndexer(indexer, i);
            shuffleTransBuilder.BuildFadeOutTransition(qs, ref fadeOutWriteBuffer);
        }
        else
        { 
            NativeArray<QST_Segment> fadeInWriteBuffer =
                OutputQSTransitionsBuffer.GetBufferSegmentAndWriteIndexer(indexer, i);
            shuffleTransBuilder.BuildFadeInTransition(qs, ref fadeInWriteBuffer);
        }
    }
}