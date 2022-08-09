using System;

using UnityEngine;

/// <summary>
/// Segment of the Wheel
/// </summary>
public class Segment : MonoBehaviour
{
    private SegmentMover _segmentMover;
    // private SegmentSelectionRespond _selectionRespond;

    public MeshFilter MeshContainer { get { return _segmentMover.MeshContainer; } }

    private void Awake()
    {
        TryGetComponent(out _segmentMover);
    }

    public void StartProcessingMove(
        SegmentMove move,
        float lerpSpeed,
        Action<SegmentMove> OnSegmentCompletedMove)
    {
        _segmentMover.StartMove(move, lerpSpeed, OnSegmentCompletedMove);
    }

    public void CompleteProcessingMove()
    {
        
    }

    public void Dissappear()
    {
        Destroy(gameObject);
    }
}
