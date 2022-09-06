using System.Collections;

using Unity.Jobs;
using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;

using Orazum.Collections;
using Orazum.Meshing;

public abstract class FigureGeneratorTransitions : MonoBehaviour
{
    protected JobHandle _dataJobHandle;
    protected QS_TransitionsBuffer _transitionsCollection;

    public void StartGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency)
    {
        StartTransitionsGeneration(quadStripsCollection, dependency);
        
    }
    protected abstract void StartTransitionsGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency);
    
    private QS_TransitionsBuffer _shuffleTransitionsCollection;
    private JobHandle _shuffleTransitionsJobHandle;
    private void StartShuffleTransitionsGeneration(in QuadStripsBuffer quadStripCollection)
    {
        NativeArray<QST_Segment> buffer = new NativeArray<QST_Segment>(quadStripCollection.GetQuadCount() * 2, Allocator.Persistent);
        NativeArray<int2> bufferIndexers = new NativeArray<int2>(quadStripCollection.QuadStripsCount * 2, Allocator.TempJob);
        _shuffleTransitionsCollection = new QS_TransitionsBuffer(buffer, bufferIndexers);

        FigureGenJobShuffleTrans job = new FigureGenJobShuffleTrans()
        {
            InputQuadStripsCollection = quadStripCollection,
            OutputQSTransSegmentBuffer = _shuffleTransitionsCollection
        };
        _shuffleTransitionsJobHandle = job.ScheduleParallel(_shuffleTransitionsCollection.TransitionsCount, 8, default);
        bufferIndexers.Dispose(_shuffleTransitionsJobHandle);
    }

    public void FinishGeneration(Figure figure)
    {
        FinishTransitionsGeneration(figure);
        FinishShuffleTransitionsGeneration(figure);

    }
    protected abstract void FinishTransitionsGeneration(Figure figure);

    private void FinishShuffleTransitionsGeneration(Figure figure)
    {
        _shuffleTransitionsJobHandle.Complete();
        Array2D<FigureShuffleTransition> shuffleTransitions = new Array2D<FigureShuffleTransition>(figure.Dimensions);
        for (int i = 0; i < _shuffleTransitionsCollection.TransitionsCount; i++)
        {
            QS_Transition fadeOut = _shuffleTransitionsCollection.GetQSTransition(i);
            QS_Transition fadeIn = _shuffleTransitionsCollection.GetQSTransition(i);

            FigureShuffleTransition transData = new FigureShuffleTransition();

            FigureShuffleTransition.FadeOut(ref transData) = fadeOut;
            FigureShuffleTransition.FadeIn(ref transData) = fadeIn;
        }
        figure.AssignShuffleTransitions(shuffleTransitions);
    }


    protected virtual void OnDestroy()
    {
        _transitionsCollection.DisposeIfNeeded();
    }
}

public struct FigureShuffleTransition
{
    private QS_Transition fadeOut;
    private QS_Transition fadeIn;

    public static ref QS_Transition FadeOut(ref FigureShuffleTransition instance)
    {
        return ref instance.fadeOut;
    }

    public static ref QS_Transition FadeIn(ref FigureShuffleTransition instance)
    {
        return ref instance.fadeIn;
    }
}
