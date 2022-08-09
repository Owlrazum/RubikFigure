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

    private Material _emptyMaterial;
    private Material _highlightMaterial;

    private void Awake()
    {
        TryGetComponent(out _meshFilter);
        TryGetComponent(out _meshCollider);
        TryGetComponent(out _meshRenderer);
    }

    public SegmentPointCornerPositions CornerPositions { get; private set; }
    public Vector3 Position { get; private set; }
    public void Initialize(SegmentPointCornerPositions cornerPositionsArg, 
        Material emptyMaterialArg, Material highlightMaterialArg)
    {
        Assert.IsNotNull(Segment);
        CornerPositions = cornerPositionsArg;
        float3 center = (CornerPositions.BBL + CornerPositions.FTR) / 2;
        Position = center;

        _meshFilter.mesh = Instantiate(Segment.MeshContainer.mesh);
        _meshCollider.sharedMesh = Instantiate(Segment.MeshContainer.mesh);
        _meshCollider.isTrigger = true;

        _emptyMaterial = emptyMaterialArg;
        _highlightMaterial = highlightMaterialArg;
        _meshRenderer.sharedMaterial = _emptyMaterial;
    }

    public Segment Segment { get; set; }

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