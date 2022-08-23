using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Assertions;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FigureSegmentPoint : MonoBehaviour
{
    private MeshFilter _meshFilter;
    private MeshRenderer _meshRenderer;

    [SerializeField]
    private Material _emptyMaterial;

    private void Awake()
    {
        TryGetComponent(out _meshFilter);
        TryGetComponent(out _meshRenderer);

        _meshRenderer.sharedMaterial = _emptyMaterial;
    }

    public int2 Index { get; private set; }
    public void AssignIndex(int2 index)
    {
        Index = index;
    }

    public FigureSegment Segment { get; set; }
    public void InitializeWithSingleMesh(Mesh mesh)
    {
        Assert.IsNotNull(Segment.MeshContainer.mesh);
        _meshFilter.mesh = mesh;
        var collider = gameObject.AddComponent<FigureSegmentPointCollider>();
        collider.Initialize(mesh, this);
    }

    public void InitializeWithMultiMesh(Mesh renderMesh, Mesh[] colliderMultiMesh)
    {
        Assert.IsNotNull(Segment.MeshContainer.mesh);
        _meshFilter.mesh = renderMesh;

        for (int i = 0; i < colliderMultiMesh.Length; i++)
        {
            GameObject meshSegment = new GameObject($"pointSegment_{i}");
            meshSegment.layer = gameObject.layer;
            var collider = meshSegment.AddComponent<FigureSegmentPointCollider>();
            collider.Initialize(colliderMultiMesh[i], this);
            meshSegment.transform.parent = transform;
        }
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