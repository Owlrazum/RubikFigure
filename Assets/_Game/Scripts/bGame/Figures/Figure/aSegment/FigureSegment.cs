using System;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// Segment of the Wheel
/// </summary>
[RequireComponent(typeof(FigureSegmentRenderer))]
public abstract class FigureSegment : MonoBehaviour
{
    protected FigureSegmentMover _mover;
    protected FigureSegmentRenderer _renderer;
    protected int _puzzleIndex;

    public MeshFilter MeshContainer { get { return _mover.MeshContainer; } }
    public FigureSegmentMover Mover { get { return _mover; } }
    public int PuzzleIndex { get { return _puzzleIndex; } }

    private void Awake()
    {
        InitializeMover();
        Assert.IsNotNull(_mover);
        TryGetComponent(out _renderer);
    }
    protected abstract void InitializeMover();
    
    public virtual void Initialize(float2 uv, int meshResolution, int puzzleIndexArg)
    { 
        _mover.Initialize(uv, meshResolution);
        _puzzleIndex = puzzleIndexArg;
    }

    public void StartMove(
        FigureSegmentMove move,
        Action OnMoveToDestinationCompleted)
    {
        _mover.StartMove(move, OnMoveToDestinationCompleted);
    }

    public void HighlightRender()
    {
        _renderer.Highlight();
    }

    public void DefaultRender()
    {
        _renderer.Default();
    }

    public void Dissappear()
    {
        gameObject.SetActive(false);
    }
}
