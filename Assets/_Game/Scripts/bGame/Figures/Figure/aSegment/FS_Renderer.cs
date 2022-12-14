using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class FS_Renderer : MonoBehaviour
{
    private Material _defaultMaterial;
    private Material _highlightMaterial;

    private MeshRenderer _meshRenderer;

    public void Initialize(Material defaultMaterial, Material highlightMaterial)
    {
        _defaultMaterial = defaultMaterial;
        _highlightMaterial = highlightMaterial;

        TryGetComponent(out _meshRenderer);
        _meshRenderer.material = _defaultMaterial;
    }

    [ContextMenu("Highlight")]
    public void Highlight()
    {
        Debug.Log("Highlight");
        _meshRenderer.material = _highlightMaterial;
    }

    public void Default()
    {
        _meshRenderer.material = _defaultMaterial;
    }
}