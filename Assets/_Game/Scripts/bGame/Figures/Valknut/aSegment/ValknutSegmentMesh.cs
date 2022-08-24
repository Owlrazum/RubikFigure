using UnityEngine.Assertions;
using Unity.Mathematics;
using Orazum.Math;
using static Orazum.Math.MathUtilities;

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
        Assert.IsTrue(stripSegmentsCount >= 0 && stripSegmentsCount <= 4);
        _stripSegmentsCount = stripSegmentsCount;

        _s1 = new float2x2(stripsData[0].xy, stripsData[0].zw);
        _s2 = new float2x2(stripsData[1].xy, stripsData[1].zw);
        _s3 = new float2x2(stripsData[2].xy, stripsData[2].zw);
        _s4 = new float2x2(stripsData[3].xy, stripsData[3].zw);
    }

    public float4x2 GetRays(ClockOrderType clockOrder, DirectionOrderType directionOrder)
    {
        float2x2 start = float2x2.zero;
        float2x2 end = float2x2.zero;
        switch (directionOrder)
        {
            case DirectionOrderType.Start:
                GetSegmentsForStartRay(out start, out end);
                break;
            case DirectionOrderType.End:
                GetSegmentsForEndRay(clockOrder, out start, out end);
                break;
            default:
                throw new System.ArgumentOutOfRangeException("Unknown direction type");
        }

        return GetSegmentRays(in start, in end);
    }

    // we use only cw order because of the way the valknut transitions work.
    // there is no need in computing the end;
    private void GetSegmentsForStartRay(out float2x2 start, out float2x2 end)
    { 
        start = _s2;
        end = _s1;
    }

    private void GetSegmentsForEndRay(ClockOrderType clockOrder, out float2x2 start, out float2x2 end)
    {
        start = float2x2.zero;
        end = float2x2.zero;
        switch (clockOrder)
        { 
            case ClockOrderType.CW:
            int lastIndex = _stripSegmentsCount - 1;
                switch (lastIndex)
                { 
                    case 2:
                        start = _s2;
                        end = _s3;
                        break;
                    case 3:
                        start = _s3;
                        end = _s4;
                        break;
                }
                break;
            case ClockOrderType.CCW:
                start = _s2;
                end = _s1;
                break;
            default:
                throw new System.ArgumentOutOfRangeException("Unknown clockOrder type");
        }
    }
}