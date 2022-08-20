using Unity.Mathematics;

public struct WheelSegmentMesh
{
    private float3 _startRay;
    private float _innerRadius;
    private float _outerRadius;
    private float _angleDelta;
    private int _indicesCount;

    public int Count { get; private set; }

    /// <summary>
    /// data: xy:inner outer radiuses, z:finalAngle, w:lerpDeltaRelativeToResolution
    /// </summary>
    public WheelSegmentMesh(float3x2 data, int segmentResolution)
    { 
        _startRay = data[0];
        
        _innerRadius = data[1].x;
        _outerRadius = data[1].y;
        _angleDelta = data[1].z;

        _indicesCount = (segmentResolution) * 4;
        Count = (segmentResolution + 1) * 2;
    }

    public float3 GetPointVertexPos(int pointVertexIndex)
    {
        int rayIndex = pointVertexIndex / 2;
        float angle = _angleDelta * rayIndex;
        quaternion q = quaternion.AxisAngle(math.up(), angle);
        float3 vertexRay = math.rotate(q, _startRay);

        bool isInner = IsInnerVertex(pointVertexIndex);
        return vertexRay * (isInner ? _innerRadius : _outerRadius);
    }

    private bool IsInnerVertex(int pointVertexIndex)
    {
        return pointVertexIndex % 2 == 0;
    }
}