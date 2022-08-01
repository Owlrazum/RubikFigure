using UnityEngine;

public class Wheel : MonoBehaviour
{
    private MeshFilter[] _segmentMeshFilters;

    public void Initialize(MeshFilter[] segmentMeshFiltersArg)
    {
        _segmentMeshFilters = segmentMeshFiltersArg;
    }

    public Mesh GetMesh(int index)
    {
        return _segmentMeshFilters[index].mesh;
    }

    public void AssignMesh(Mesh meshArg, int index)
    { 
        _segmentMeshFilters[index].mesh = meshArg;
    }
}