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
    public static int VertexCount { get; private set; }
    public static void InitializeVertexCount(int vertexCount)
    {
        VertexCount = vertexCount;
    }

    private SegmentMover _segmentMover;
    private SegmentRenderer _segmentRenderer;
    // private SegmentSelectionRespond _selectionRespond;

    public MeshFilter MeshContainer { get { return _segmentMover.MeshContainer; } }
    private int _puzzleIndex;
    public int PuzzleIndex{ get { return _puzzleIndex; } }

    private int2 _segmentIndex;

    private void Awake()
    { 
        TryGetComponent(out _segmentMover);
        TryGetComponent(out _segmentRenderer);
    }

    public void Initialize(NativeArray<VertexData> verticesArg, int puzzleColorIndexArg)
    { 
        _segmentMover.Initialize(verticesArg);
        _puzzleIndex = puzzleColorIndexArg;
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
        gameObject.SetActive(false);
    }

    public void Appear()
    {
        print("hi");
        gameObject.SetActive(true);
    }
}
