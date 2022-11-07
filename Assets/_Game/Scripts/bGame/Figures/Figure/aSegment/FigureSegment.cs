using System;

using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Assertions;

// FS is reserved
public class FigureSegment : MonoBehaviour
{
    protected FS_Mover _mover;
    protected FS_Renderer _renderer;
    protected int _puzzleIndex;

    public MeshFilter MeshContainer { get { return _mover.MeshContainer; } }
    public FS_Mover Mover { get { return _mover; } }
    public int PuzzleIndex { get { return _puzzleIndex; } }

    private void Awake()
    {
        _mover = gameObject.AddComponent<FS_Mover>();
        _renderer = gameObject.AddComponent<FS_Renderer>();
    }

    public void AssignPuzzleIndex(int puzzleIndex)
    { 
        _puzzleIndex = puzzleIndex;
    }
    public void PrepareMover(float2 uv, int2 meshBuffersMaxCount)
    { 
        _mover.Initialize(uv, meshBuffersMaxCount);
    }
    public void PrepareRenderer(Material defaultMaterial, Material highlightMaterial)
    {
        _renderer.Initialize(defaultMaterial, highlightMaterial);
    }

    public void StartMove(
        FigureMoveOnSegment move,
        Action onMoveCompleted)
    {
        _mover.StartMove(move, onMoveCompleted);
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
