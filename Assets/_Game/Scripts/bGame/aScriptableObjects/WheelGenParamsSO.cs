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
    private int _ringCount;
    public int RingCount { get { return _ringCount; } }

    [SerializeField]
    private int _segmentResolution;
    public int SegmentResolution { get { return _segmentResolution; } }
}