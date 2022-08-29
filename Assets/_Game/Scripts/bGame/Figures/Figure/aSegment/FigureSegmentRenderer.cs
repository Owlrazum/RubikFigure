using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FigureSegmentRenderer : MonoBehaviour
{
    [SerializeField]
    private Material _defaultMaterial;
    
    [SerializeField]
    private Material _highlightMaterial;

    private MeshRenderer _meshRenderer;

    private void Awake()
    {
        TryGetComponent(out _meshRenderer);
        _meshRenderer.material = _defaultMaterial;
    }

    [ContextMenu("Print")]
    public void Highlight()
    {
        _meshRenderer.material = _highlightMaterial;
    }

    public void Default()
    {
        _meshRenderer.material = _defaultMaterial;
    }
}