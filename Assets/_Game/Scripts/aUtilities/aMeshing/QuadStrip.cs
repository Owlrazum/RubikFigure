using System;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Math;
using static Orazum.Math.LineSegmentUtilities;

namespace Orazum.Meshing
{ 
    public struct QuadStrip : IDisposable
    {
        private NativeArray<float3x2> _lineSegments;

        public float3x2 this[int index]
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

        public QuadStrip(NativeArray<float3x2> lineSegments)
        {
            Assert.IsTrue(lineSegments.Length > 1);
            _lineSegments = lineSegments;
        }

        public float ComputeSingleLength()
        {
            float singleLength = 0;
            for (int i = 0; i < QuadsCount; i++)
            {
                singleLength += DistanceLineSegment(_lineSegments[i][0], _lineSegments[i + 1][0]);
            }
            return singleLength;
        }

        public float2 ComputeDoubleLength()
        {
            float2 doubleLength = float2.zero;
            for (int i = 0; i < QuadsCount; i++)
            {
                // perhaps separate variable shuold be created.
                doubleLength += DistanceLineSegments(_lineSegments[i], _lineSegments[i + 1]);
            }
            return doubleLength;
        }

        public float3x4 GetRays(LineEndType quadStripEnd, LineEndDirectionType raysDirection)
        {
            float3x2 startSegment, endSegment;

            if (quadStripEnd == LineEndType.Start)
            { 
                GetSegmentsForStartRay(raysDirection, out startSegment, out endSegment);
            }
            else
            { 
                GetSegmentsForEndRay(raysDirection, out startSegment, out endSegment);
            }

            return RaysUtilities.GetSegmentRays(in startSegment, in endSegment);
        }

        public float3x2 GetRay(LineEndType quadStripEnd, LineEndType lineSegmentEnd, LineEndDirectionType rayDirection)
        {
            float3x2 startSegment, endSegment;

            if (quadStripEnd == LineEndType.Start)
            { 
                GetSegmentsForStartRay(rayDirection, out startSegment, out endSegment);
            }
            else
            { 
                GetSegmentsForEndRay(rayDirection, out startSegment, out endSegment);
            }

            if (lineSegmentEnd == LineEndType.Start)
            { 
                return RaysUtilities.RayFromDelta(startSegment[0], endSegment[0]);
            }
            else
            { 
                return RaysUtilities.RayFromDelta(startSegment[1], endSegment[1]);
            }
        }

        public void Dispose()
        {
            _lineSegments.Dispose();
        }

        private void GetSegmentsForStartRay(LineEndDirectionType raysDirection, out float3x2 start, out float3x2 end)
        {
            if (raysDirection == LineEndDirectionType.StartToEnd)
            { 
                start = _lineSegments[0];
                end = _lineSegments[1];
            }
            else
            { 
                end = _lineSegments[0];
                start = _lineSegments[1];
            }
        }

        private void GetSegmentsForEndRay(LineEndDirectionType raysDirection, out float3x2 start, out float3x2 end)
        {
            if (raysDirection == LineEndDirectionType.StartToEnd)
            { 
                start = _lineSegments[LineSegmentsCount - 2];
                end = _lineSegments[LineSegmentsCount - 1];
            }
            else
            { 
                end = _lineSegments[LineSegmentsCount - 1];
                start = _lineSegments[LineSegmentsCount - 2];
            }
        }

        public void DrawDebug(Color color, float duration, float3 offset = default)
        {
            float3x2 s1 = _lineSegments[0];
            float3x2 s2 = _lineSegments[1];
            Debug.DrawLine(s1[0] + offset, s1[1] + offset, color, duration);
            for (int i = 0; i < LineSegmentsCount - 1; i++)
            {
                s1 = _lineSegments[i];
                s2 = _lineSegments[i + 1];
                Debug.DrawLine(s1[0] + offset, s2[0] + offset, color, duration);
                Debug.DrawLine(s1[1] + offset, s2[1] + offset, color, duration);
            }
            Debug.DrawLine(s2[0] + offset, s2[1] + offset, color, duration);
        }
    }
}