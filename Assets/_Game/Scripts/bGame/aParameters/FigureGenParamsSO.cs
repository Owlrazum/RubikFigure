using Unity.Mathematics;
using UnityEngine;

public abstract class FigureGenParamsSO : ScriptableObject
{
    [SerializeField]
    private float _height;
    public float Height { get { return _height; } }

    public abstract int2 Dimensions {get;}

    [SerializeField]
    private GameObject _segmentPrefab;
    public GameObject SegmentPrefab { get { return _segmentPrefab; } }

    [SerializeField]
    private GameObject _segmentPointPrefab;
    public GameObject SegmentPointPrefab { get { return _segmentPointPrefab; } }
}