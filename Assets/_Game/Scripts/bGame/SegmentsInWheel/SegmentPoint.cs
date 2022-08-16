using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshCollider))]
[RequireComponent(typeof(MeshRenderer))]
public class SegmentPoint : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private MeshCollider _meshCollider;
    private MeshRenderer _meshRenderer;

    [SerializeField]
    private Material _emptyMaterial;

    private void Awake()
    {
        TryGetComponent(out _meshFilter);
        TryGetComponent(out _meshCollider);
        TryGetComponent(out _meshRenderer);
    }

    public Segment Segment { get; set; }
    public int2 Index { get; private set; }
    public void InitializeAfterMeshesGenerated(Mesh mesh, Segment segment, int2 index)
    {
        Assert.IsNotNull(segment.MeshContainer.mesh);
        Segment = segment;
        Index = index;

        _meshFilter.mesh = mesh;
        _meshCollider.sharedMesh = mesh;
        _meshCollider.isTrigger = true;

        _meshRenderer.sharedMaterial = _emptyMaterial;
    }

    public override string ToString()
    {
        if (Segment == null)
        { 
            return "0";
        }
        else
        {
            return "1";
        }
    }
}