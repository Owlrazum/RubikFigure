using Unity.Mathematics;
using UnityEngine;

public abstract class FigureGenParamsSO : ScriptableObject
{
    [SerializeField]
    private float _segmentPointHeight = 1;
    public float SegmentPointHeight { get { return _segmentPointHeight; } }

    public abstract int2 Dimensions {get;}

    [SerializeField]
    private GameObject _segmentPrefab;
    public GameObject SegmentPrefab { get { return _segmentPrefab; } }

    [SerializeField]
    private GameObject _segmentPointPrefab;
    public GameObject SegmentPointPrefab { get { return _segmentPointPrefab; } }
}