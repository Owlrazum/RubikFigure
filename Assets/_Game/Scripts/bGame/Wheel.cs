using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wheel : MonoBehaviour
{
    [SerializeField]
    private float _testSpeed = 1;

    private UIButton _shuffleButton;
    private MeshFilter[] _segmentMeshFilters;

    private int[] _state;
    private int _segmentCountInOneSide;
    private int _sideCount;

    public void MeshDataInit(MeshFilter[] segmentMeshFiltersArg, int sideCountArg, int segmentCountInOneSideArg)
    {
        _segmentMeshFilters = segmentMeshFiltersArg;

        _segmentCountInOneSide = segmentCountInOneSideArg;
        _sideCount = sideCountArg;
        _state = new int[_sideCount * _segmentCountInOneSide];
        for (int i = 0; i < _state.Length; i++)
        {
            _state[i] = i / _segmentCountInOneSide;
        }
    }

    public void GeneralInit(UIButton shuffleButtonArg)
    {
        _shuffleButton = shuffleButtonArg;
        _shuffleButton.EventOnTouch += Shuffle;
    }

    private void OnDestroy()
    {
        _shuffleButton.EventOnTouch -= Shuffle;
    }

    private void Shuffle()
    {
        print("Starting shuffle");
        StartCoroutine(TestMultipleSelection());
    }

    private IEnumerator TestMultipleSelection()
    {
        while (true)
        {
            for (int side = 0; side < _sideCount; side++)
            {
                int nextRayIndex = side + 1 < _sideCount ? side + 1 : 0;
                Vector2Int rayIndices = new Vector2Int(side, nextRayIndex);

                for (int s = 0; s < _segmentCountInOneSide; s++)
                {
                    int segmentIndex = side * _segmentCountInOneSide + s;
                    Mesh mesh = _segmentMeshFilters[segmentIndex].mesh;
                    Vector3[] vertices = mesh.vertices;
                    WheelUtilities.TestSegment(vertices, _testSpeed, rayIndices);
                    mesh.vertices = vertices;
                    _segmentMeshFilters[segmentIndex].mesh = mesh;
                }
            }
            yield return null;
        }
    }

    private int Index(int x, int y)
    {
        return y * _segmentCountInOneSide + x;
    }
}