using Unity.Mathematics;

/// <summary>
/// Segment meshes are constructed using quads, 
/// while this struct uses indexing based on rays:
/// inner outer, shift angle, inner outer, shift angle, inner outer ...
/// </summary>
public struct SegmentVertexPositions
{
    private float3 _startRay;
    private float4 _data;
    private int _indicesCount;

    public int Count { get; private set; }

    /// <summary>
    /// data: xy:inner outer radiuses, z:finalAngle, w:lerpDeltaRelativeToResolution
    /// </summary>
    public SegmentVertexPositions(float3 startRay, float4 data, int segmentResolution)
    { 
        _startRay = startRay;
        _data = data;
        _indicesCount = 4 + (segmentResolution - 1) * 3; // the total count is (segRes + 1) * 2
        Count = (segmentResolution + 1) * 2;
    }

    /// <summary>
    /// Argument is in other words an index in this struct's convention
    /// </summary>
    public float3 GetPointVertexPos(int pointVertexIndex)
    {
        float radius;
        if (IsInnerVertex(pointVertexIndex))
        {
            radius = _data.x;
        }
        else
        {
            radius = _data.y;
        }
        int rayIndex = pointVertexIndex / 2;
        float angle = math.lerp(0, _data.z, _data.w * rayIndex);
        quaternion q = quaternion.AxisAngle(math.up(), angle);
        float3 vertexRay = math.rotate(q, _startRay);

        return vertexRay * radius;
    }

    /// <summary>
    /// Argument is in other words an index in this struct's convention
    /// </summary>
    public int2 GetSegmentIndices(int pointVertexIndex)
    {
        int2 indices = new int2(-1, -1);
        if (IsStartPointVertex(pointVertexIndex))
        {
            indices.x = pointVertexIndex;
            return indices;
        }

        int segmentVertexIndex = (pointVertexIndex / 2) * 4;
        bool isInnerVertex = IsInnerVertex(pointVertexIndex);
        if (!isInnerVertex)
        {
            segmentVertexIndex++;
        }

        indices.x = segmentVertexIndex - (isInnerVertex ? 1 : 3);
        if (IsEndPointVertex(segmentVertexIndex))
        {
            return indices;
        }

        indices.y = segmentVertexIndex;
        return indices;
    }

    private bool IsInnerVertex(int pointVertexIndex)
    {
        return pointVertexIndex % 2 == 0;
    }

    private bool IsStartPointVertex(int pointVertexIndex)
    {
        return pointVertexIndex <= 1;
    }

    private bool IsEndPointVertex(int pointVertexIndex)
    {
        return pointVertexIndex >= _indicesCount;
    }
}