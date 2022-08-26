using System;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Math;
using static Orazum.Math.MathUtilities;
public struct QuadStrip : IDisposable
{
    private NativeArray<float2x2> _lineSegments;

    public float2x2 this[int index]
    {
        get
        {
            return _lineSegments[index];
        }
    }
    public int LineSegmentsCount
    {
        get
        {
            return _lineSegments.Length;
        }
    }
    public int QuadsCount
    {
        get 
        {
            return _lineSegments.Length - 1;
        }
    }

    public QuadStrip(NativeArray<float2x2> lineSegments)
    {
        Assert.IsTrue(lineSegments.Length > 1);
        _lineSegments = lineSegments;
    }

    public float4x2 GetRays(LineEndType quadStripEnd, LineEndDirectionType raysDirection)
    {
        float2x2 start = float2x2.zero;
        float2x2 end = float2x2.zero;
        switch (quadStripEnd)
        {
            case LineEndType.Start:
                GetSegmentsForStartRay(raysDirection, out start, out end);
                break;
            case LineEndType.End:
                GetSegmentsForEndRay(raysDirection, out start, out end);
                break;
            default:
                throw new System.ArgumentOutOfRangeException("Unknown LineEndType");
        }

        return GetSegmentRays(in start, in end);
    }

    public float4 GetRay(LineEndType quadStripEnd, LineEndType lineSegmentEnd, LineEndDirectionType rayDirection)
    {
        float2x2 startSegment;
        float2x2 endSegment;

        switch (quadStripEnd)
        { 
            case LineEndType.Start:
                GetSegmentsForStartRay(rayDirection, out startSegment, out endSegment);
                break;
            case LineEndType.End:
                GetSegmentsForEndRay(rayDirection, out startSegment, out endSegment);
                break;
            default:
                throw new System.ArgumentOutOfRangeException("UnknonwLineEndType");
        }

        switch (lineSegmentEnd)
        { 
            case LineEndType.Start:
                return RayFromDelta(startSegment[0], endSegment[0]);
            case LineEndType.End:
                return RayFromDelta(startSegment[1], endSegment[1]);
            default:
                throw new System.ArgumentOutOfRangeException("UnknonwLineEndType");
        }
    }

    public void Dispose()
    {
        _lineSegments.Dispose();
    }

    private void GetSegmentsForStartRay(LineEndDirectionType raysDirection, out float2x2 start, out float2x2 end)
    {
        switch (raysDirection)
        { 
            case LineEndDirectionType.StartToEnd:
                start = _lineSegments[0];
                end = _lineSegments[1];
                return;
            case LineEndDirectionType.EndToStart:
                end = _lineSegments[0];
                start = _lineSegments[1];
                return;
        }

        throw new System.ArgumentOutOfRangeException("Unknown LineEndDirectionType");
    }

    private void GetSegmentsForEndRay(LineEndDirectionType raysDirection, out float2x2 start, out float2x2 end)
    {
        start = float2x2.zero;
        end = float2x2.zero;
        switch (raysDirection)
        { 
            case LineEndDirectionType.StartToEnd:
                start = _lineSegments[LineSegmentsCount - 2];
                end = _lineSegments[LineSegmentsCount - 1];
                return;
            case LineEndDirectionType.EndToStart:
                end = _lineSegments[LineSegmentsCount - 1];
                start = _lineSegments[LineSegmentsCount - 2];
                return;
        }
        
        throw new System.ArgumentOutOfRangeException("Unknown LineEndDirectionType");
    }
}