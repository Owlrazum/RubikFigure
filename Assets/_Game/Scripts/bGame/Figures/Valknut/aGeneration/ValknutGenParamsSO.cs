using UnityEngine;

[CreateAssetMenu(fileName = "ValknutGenParams", menuName = "Figure/ValknutGenParams", order = 1)]
public class ValknutGenParamsSO : FigureGenParamsSO
{
    [SerializeField]
    private float _innerTriangleRadius = 1;
    public float InnerTriangleRadius { get { return _innerTriangleRadius; } }

    [SerializeField]
    private float _width = 0.5f;
    public float Width { get { return _width; } }

    [SerializeField]
    private float _gapSize = 0.4f;
    public float GapSize { get { return _gapSize; } }
}