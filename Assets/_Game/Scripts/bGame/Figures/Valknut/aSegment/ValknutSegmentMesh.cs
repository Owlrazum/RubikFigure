using UnityEngine.Assertions;
using Unity.Mathematics;

public struct ValknutSegmentMesh
{
    private int _stripSegmentsCount;
    public int StripSegmentsCount { get { return _stripSegmentsCount; } }

    private float2x2 _s1;
    private float2x2 _s2;
    private float2x2 _s3;
    private float2x2 _s4;

    public float2x2 this[int index]
    {
        get
        {
            switch (index)
            {
                case 0:
                    return _s1;
                case 1:
                    return _s2;
                case 2:
                    return _s3;
                case 3:
                    return _s4;
            }
            throw new System.ArgumentOutOfRangeException($"Index {index} is should be in [0..StripSegmentsCount = {StripSegmentsCount})");
        }
        private set
        {
            switch (index)
            {
                case 0:
                    _s1 = value;
                    break;
                case 1:
                    _s2 = value;
                    break;
                case 2:
                    _s3 = value;
                    break;
                case 3:
                    _s4 = value;
                    break;
            }
        }
    }

    public ValknutSegmentMesh(in float4x4 stripsData, int stripSegmentsCount)
    {
        Assert.IsTrue(stripSegmentsCount >= 0 && stripSegmentsCount < 4);
        _stripSegmentsCount = stripSegmentsCount;

        _s1 = new float2x2(stripsData[0].xy, stripsData[0].zw);
        _s2 = new float2x2(stripsData[1].xy, stripsData[1].zw);
        _s3 = new float2x2(stripsData[2].xy, stripsData[2].zw);
        _s4 = new float2x2(stripsData[3].xy, stripsData[3].zw);
    }
}