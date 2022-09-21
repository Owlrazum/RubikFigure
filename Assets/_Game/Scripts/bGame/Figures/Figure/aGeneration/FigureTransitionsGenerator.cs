using System;
using System.Collections;

using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;

using Orazum.Collections;
using Orazum.Meshing;

public abstract class FigureTransitionsGenerator : MonoBehaviour
{
    protected struct QST_NativeData : IDisposable
    {
        public NativeArray<int2> IndexersBuffer;
        public NativeArray<QST_Segment> SegmentsBuffer;
        public QS_TransitionsBuffer TransitionsBuffer;

        public void Dispose()
        {
            TransitionsBuffer.Dispose();
        }

        public void DisposeIfNeeded()
        { 
            TransitionsBuffer.DisposeIfNeeded();
        }
    }

    protected JobHandle _jobHandle;
    protected QST_NativeData qst_data;

    private NativeArray<int2> _fadeInOutTransitionsBufferIndexers;

    public void StartGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency)
    {
        StartTransitionsGeneration(quadStripsCollection, dependency);
        StartShuffleTransitionsGeneration(quadStripsCollection);
    }
    protected abstract void StartTransitionsGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency);
    
    private QS_TransitionsBuffer _shuffleTransitionsCollection;
    private JobHandle _shuffleTransitionsJobHandle;
    private void StartShuffleTransitionsGeneration(in QuadStripsBuffer quadStripCollection)
    {
        _fadeInOutTransitionsBufferIndexers = BufferUtils.GetFadeInOutTransitionsBufferIndexers(quadStripCollection);

        NativeArray<QST_Segment> buffer = new NativeArray<QST_Segment>(quadStripCollection.GetQuadCount() * 2, Allocator.Persistent);
        NativeArray<int2> bufferIndexers = new NativeArray<int2>(quadStripCollection.QuadStripsCount * 2, Allocator.Persistent);
        _shuffleTransitionsCollection = new QS_TransitionsBuffer(buffer, bufferIndexers);

        FigureUniversalTransitionsGenJob job = new FigureUniversalTransitionsGenJob()
        {
            InputQuadStripsCollection = quadStripCollection,
            InputQSTransitionsBufferIndexers = _fadeInOutTransitionsBufferIndexers,
            OutputQSTransitionsBuffer = _shuffleTransitionsCollection
        };
        _shuffleTransitionsJobHandle = job.ScheduleParallel(_shuffleTransitionsCollection.TransitionsCount, 8, default);
    }

    public void FinishGeneration(Figure figure)
    {
        FinishTransitionsGeneration(figure);
        FinishShuffleTransitionsGeneration(figure);
        _fadeInOutTransitionsBufferIndexers.Dispose();

    }
    protected abstract void FinishTransitionsGeneration(Figure figure);

    // Big assumption here: quadStripCollection, hence shuffleTransitionsCollection are placed in 
    // first increasing Dimensions.y, then increasing Dimensions.x
    private void FinishShuffleTransitionsGeneration(Figure figure)
    {
        _shuffleTransitionsJobHandle.Complete();
        Array2D<FadeOutInTransitions> shuffleTransitions = new Array2D<FadeOutInTransitions>(figure.Dimensions);
        int2 indexer = int2.zero;
        for (int i = 0; i < _shuffleTransitionsCollection.TransitionsCount; i += 2)
        {
            QS_Transition fadeOut = _shuffleTransitionsCollection.GetQSTransition(i);
            QS_Transition fadeIn = _shuffleTransitionsCollection.GetQSTransition(i + 1);

            // print($"fadeOut {fadeOut.Length} :: fadeIn {fadeIn.Length}");
            FadeOutInTransitions transData = new FadeOutInTransitions();
            transData.FadeOut = fadeOut;
            transData.FadeIn = fadeIn;
            shuffleTransitions[indexer] = transData;
            indexer.y++;
            if (indexer.y >= figure.Dimensions.y)
            {
                indexer.y = 0;
                indexer.x++;
            }
            // print($"transData {FigureShuffleTransition.FadeOut(ref transData)} {FigureShuffleTransition.FadeIn(ref transData)}");
        }
        figure.AssignUniversalTransitions(shuffleTransitions);
    }


    protected virtual void OnDestroy()
    {
        qst_data.DisposeIfNeeded();
        _shuffleTransitionsCollection.DisposeIfNeeded();
        CollectionUtilities.DisposeIfNeeded(_fadeInOutTransitionsBufferIndexers);
    }
}
