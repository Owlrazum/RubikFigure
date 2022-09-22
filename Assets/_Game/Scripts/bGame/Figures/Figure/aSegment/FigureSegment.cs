using System;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

public class FigureSegment : MonoBehaviour
{
    protected FigureSegmentMover _mover;
    protected FigureSegmentRenderer _renderer;
    protected int _puzzleIndex;

    public MeshFilter MeshContainer { get { return _mover.MeshContainer; } }
    public FigureSegmentMover Mover { get { return _mover; } }
    public int PuzzleIndex { get { return _puzzleIndex; } }

    private void Awake()
    {
        _mover = gameObject.AddComponent<FigureSegmentMover>();
        bool isFound = TryGetComponent<FigureSegmentRenderer>(out _renderer);
        Assert.IsTrue(isFound);
    }

    public virtual void Initialize(float2 uv, int puzzleIndex, int2 meshBuffersMaxCount)
    { 
        _mover.Initialize(uv, meshBuffersMaxCount);
        _puzzleIndex = puzzleIndex;
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

    public void Appear()
    {
        gameObject.SetActive(true);
    }

    public void Dissappear()
    {
        gameObject.SetActive(false);
    }
}
