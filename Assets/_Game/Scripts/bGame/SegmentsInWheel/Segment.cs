using System;

using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

/// <summary>
/// Segment of the Wheel
/// </summary>
[RequireComponent(typeof(SegmentMover), typeof(SegmentRenderer))]
public class Segment : MonoBehaviour
{
    private SegmentMover _segmentMover;
    private SegmentRenderer _segmentRenderer;
    // private SegmentSelectionRespond _selectionRespond;

    public MeshFilter MeshContainer { get { return _segmentMover.MeshContainer; } }
    private int _puzzleColorIndex;
    public int PuzzleColorIndex{ get { return _puzzleColorIndex; } }

    private int2 _segmentIndex;

    private void Awake()
    { 
        TryGetComponent(out _segmentMover);
        TryGetComponent(out _segmentRenderer);
    }

    public void Initialize(NativeArray<VertexData> verticesArg, int puzzleColorIndexArg)
    { 
        _segmentMover.Initialize(verticesArg);
        _puzzleColorIndex = puzzleColorIndexArg;
    }

    public void StartMove(
        SegmentMove move,
        float lerpSpeed,
        Action<int2> OnMoveToDestinationCompleted)
    {
        _segmentMover.StartMove(move, lerpSpeed, OnMoveToDestinationCompleted);
    }

    public void HighlightRender()
    {
        _segmentRenderer.Highlight();
    }

    public void DefaultRender()
    {
        _segmentRenderer.Default();
    }

    public void Dissappear()
    {
        Destroy(gameObject);
    }
}
