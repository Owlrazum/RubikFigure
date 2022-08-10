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

    public SegmentPointCornerPositions CornerPositions { get; private set; }
    public Vector3 Position { get; private set; }
    public int2 Index { get; private set; }
    public void InitializeAfterMeshesGenerated(SegmentPointCornerPositions cornerPositionsArg, 
        Segment segmentArg, int2 indexArg)
    {
        Assert.IsNotNull(segmentArg.MeshContainer.mesh);
        CornerPositions = cornerPositionsArg;
        Segment = segmentArg;
        Index = indexArg;

        float3 center = (CornerPositions.BBL + CornerPositions.FTR) / 2;
        Position = center;

        _meshFilter.mesh = Instantiate(Segment.MeshContainer.mesh);
        _meshCollider.sharedMesh = Instantiate(Segment.MeshContainer.mesh);
        _meshCollider.isTrigger = true;

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