using UnityEngine;

[RequireComponent(typeof(MeshCollider))]
public class FSP_Collider : MonoBehaviour
{
    private MeshCollider _meshCollider;
    private FS_Point _pointReference;
    public FS_Point ParentPoint { get { return _pointReference; } }
    public void Initialize(Mesh mesh, FS_Point pointReference)
    {
        TryGetComponent(out _meshCollider);
        _meshCollider.cookingOptions = MeshColliderCookingOptions.CookForFasterSimulation | MeshColliderCookingOptions.UseFastMidphase;
        _meshCollider.sharedMesh = mesh;
        _meshCollider.convex = true;
        _meshCollider.isTrigger = true;

        _pointReference = pointReference;
    }
}