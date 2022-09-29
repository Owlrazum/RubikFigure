using Unity.Mathematics;
using UnityEngine;

public abstract class FigureGenParamsSO : ScriptableObject
{
    [Header("General")]
    [SerializeField]
    private float _segmentPointHeight = 1;
    public float SegmentPointHeight { get { return _segmentPointHeight; } }

    public abstract int2 Dimensions {get;}

    [SerializeField]
    private Material _defaultMaterial;
    public Material DefaultMaterial { get { return _defaultMaterial; } }

    [SerializeField]
    private Material _highlightMaterial;
    public Material HighlightMaterial { get { return _highlightMaterial; } }
}