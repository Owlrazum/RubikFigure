using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class FigureSegmentPointCollider : MonoBehaviour
{
    private MeshCollider _meshCollider;
    private FigureSegmentPoint _pointReference;
    public FigureSegmentPoint ParentPoint { get { return _pointReference; } }
    public void Initialize(Mesh mesh, FigureSegmentPoint pointReference)
    {
        TryGetComponent(out _meshCollider);
        _meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.UseFastMidphase;
        _meshCollider.sharedMesh = mesh;
        _meshCollider.convex = true;
        _meshCollider.isTrigger = true;

        _pointReference = pointReference;
    }
}