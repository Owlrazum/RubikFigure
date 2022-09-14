using UnityEngine;
using Unity.Mathematics;

namespace Orazum.Math
{
    public static class LineSegmentUtilities
    { 
        public static float DistanceLineSegment(float3 p1, float3 p2)
        {
            return math.length(p2 - p1);
        }
        public static float DistanceLineSegment(in float3x2 lineSegment)
        {
            return DistanceLineSegment(lineSegment[0], lineSegment[1]);
        }
        public static float2 DistanceLineSegments(in float3x2 lhs, in float3x2 rhs)
        {
            return new float2(
                DistanceLineSegment(lhs[0], rhs[0]),
                DistanceLineSegment(lhs[1], rhs[1])
            );
        }

        public static float3 GetDirection(quaternion q, float3 p1, float3 p2)
        { 
            float3 direction = math.normalize(p2 - p1);
            return math.rotate(q, direction);
        }
        public static float3 GetDirection(quaternion q, in float3x2 lineSegment)
        {
            return GetDirection(q, lineSegment[0], lineSegment[1]);
        }

        public static float3 GetLineSegmentCenter(float3 p1, float3 p2)
        {
            return (p2 + p1) / 2;
        }
        public static float3 GetLineSegmentCenter(in float3x2 lineSegment)
        {
            return GetLineSegmentCenter(lineSegment[0], lineSegment[1]);
        }

        public static float3x2 RotateLineSegment(quaternion q, in float3x2 lineSegment)
        {
            return new float3x2(
                math.rotate(q, lineSegment[0]),
                math.rotate(q, lineSegment[1])
            );
        }
        public static float3x2 RotateLineSegmentAround(quaternion q, float3 center, in float3x2 lineSegment)
        {
            float3x2 toReturn = new float3x2(
                lineSegment[0] - center,
                lineSegment[1] - center
            );
            toReturn = RotateLineSegment(q, in toReturn);
            toReturn[0] = toReturn[0] + center;
            toReturn[1] = toReturn[1] + center;

            return toReturn;
        }
        public static float3x2 RotateLineSegmentAround(
            quaternion q1, quaternion q2, 
            float3x2 centers,
            in float3x2 lineSegment
        )
        {
            float3x2 toReturn = new float3x2(
                lineSegment[0] - centers[0],
                lineSegment[1] - centers[1]
            );

            toReturn[0] = math.rotate(q1, toReturn[0]);
            toReturn[1] = math.rotate(q2, toReturn[1]);

            toReturn[0] = toReturn[0] + centers[0];
            toReturn[1] = toReturn[1] + centers[1];

            return toReturn;
        }

        public static void DrawLineSegmentWithRaysUp(float3x2 lineSegment, float length, float duration)
        {
            Debug.DrawRay(lineSegment[0], Vector3.up * length, Color.red, duration);
            Debug.DrawRay(lineSegment[1], Vector3.up * length, Color.red, duration);
        }
    }
}