using System;

using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Meshing;

/// <summary>
/// Segment of the Wheel
/// </summary>
[RequireComponent(typeof(FigureSegmentRenderer))]
public abstract class FigureSegment : MonoBehaviour
{
    public static int VertexCount { get; private set; }
    public static void InitializeVertexCount(int vertexCount)
    {
        VertexCount = vertexCount;
    }

    protected FigureSegmentMover _mover;
    public FigureSegmentMover Mover { get { return _mover; } }
    protected FigureSegmentRenderer _renderer;
    // private SegmentSelectionRespond _selectionRespond;

    public MeshFilter MeshContainer { get { return _mover.MeshContainer; } }
    protected int _puzzleIndex;
    public int PuzzleIndex { get { return _puzzleIndex; } }

    private int2 _segmentIndex;

    private void Awake()
    {
        InitializeMover();
        Assert.IsNotNull(_mover);
        TryGetComponent(out _renderer);
    }
    protected abstract void InitializeMover();
    
    public virtual void Initialize(NativeArray<VertexData> verticesArg, int puzzleIndexArg)
    { 
        _mover.Initialize(verticesArg);
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
