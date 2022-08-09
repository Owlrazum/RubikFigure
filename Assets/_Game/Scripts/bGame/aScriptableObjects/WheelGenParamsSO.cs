using UnityEngine;

[CreateAssetMenu(fileName = "WheelGenParams", menuName = "Game/WheelGenParams", order = 1)]
public class WheelGenParamsSO : ScriptableObject
{
    [SerializeField]
    private float _height;
    public float Height { get { return _height; } }

    [SerializeField]
    private float _outerRadius;
    public float OuterRadius { get { return _outerRadius; } }

    [SerializeField]
    private float _innerRadius;
    public float InnerRadius { get { return _innerRadius; } }

    [SerializeField]
    private int _sideCount;
    public int SideCount { get { return _sideCount; } }

    [SerializeField]
    private int _segmentCountInOneSide;
    public int SegmentCountInOneSide { get { return _segmentCountInOneSide; } }

    [SerializeField]
    private Material _meshesMaterial;
    public Material MeshesMaterial { get { return _meshesMaterial; } }

    [SerializeField]
    private Material _emptyMaterial;
    public Material EmptyMaterial { get { return _emptyMaterial; } }

    [SerializeField]
    private Material _highlightMaterial;
    public Material HighlightMaterial { get { return _highlightMaterial; } }
}