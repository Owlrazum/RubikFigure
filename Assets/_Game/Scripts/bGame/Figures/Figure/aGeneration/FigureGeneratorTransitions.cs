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
    public abstract void StartGeneration(in QuadStripsBuffer quadStripsCollection, JobHandle dependency);
    public abstract void FinishGeneration(Figure figure);

    protected JobHandle _dataJobHandle;
    protected QSTransitionsBuffer _transitionsCollection;

    protected virtual void OnDestroy()
    {
        _transitionsCollection.DisposeIfNeeded();
    }
}