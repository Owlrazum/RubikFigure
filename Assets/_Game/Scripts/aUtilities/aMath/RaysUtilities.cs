using Unity.Mathematics;
using UnityEngine;

using static Orazum.Math.MathUtils;

namespace Orazum.Math
{
    public static class RaysUtilities
    {
        public static float2x2 RayFromDirection(float2 origin, float2 direction)
        {
            return new float2x2(origin, direction);
        }

        public static float3x2 RayFromDirection(float3 origin, float3 direction)
        {
            return new float3x2(origin, direction);
        }

        public static float2x2 RayFromDelta(float2 start, float2 end)
        {
            return new float2x2(start, math.normalize(end - start));
        }

        public static float3x2 RayFromDelta(float3 start, float3 end)
        {
            return new float3x2(start, math.normalize(end - start));
        }

        public static float2x4 GetSegmentRays(in float2x2 start, in float2x2 end)
        {
            float2x2 r1 = RayFromDelta(start[0], end[1]);
            float2x2 r2 = RayFromDelta(start[1], end[1]);
            return new float2x4(r1[0], r1[1], r2[0], r2[1]);
        }

        public static float3x4 GetSegmentRays(in float3x2 start, in float3x2 end)
        {
            float3x2 r1 = RayFromDelta(start[0], end[0]);
            float3x2 r2 = RayFromDelta(start[1], end[1]);
            return new float3x4(r1[0], r1[1], r2[0], r2[1]);
        }

        /// <summary>
        /// intersection will be on xz, and y = 0
        /// </summary>
        public static bool IntersectRays2D(float4 r1, float4 r2, out float2 intersection)
        {
            intersection = float2.zero;
            float det = MathUtils.Determinant(r1.zw, r2.zw);
            if (Mathf.Approximately(det, 0))
            {
                return false;
            }

            float2 d = new float2(r2.x - r1.x, r2.y - r1.y);

            float u = (d.y * r2.z - d.x * r2.w) / det;
            //v = (dy * ad.x - dx * ad.y) / det
            float v = (d.y * r1.z - d.x * r1.w) / det;
            if (u > 0 && v > 0)
            {
                intersection = new float2(r1.x + r1.z * u, r1.y + r1.w * u);
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool IntersectRays2D(float4 r1, float4 r2, out float3 intersection)
        {
            bool toReturn = IntersectRays2D(r1, r2, out float2 p);
            intersection = x0z(p);
            return toReturn;
        }

        public static bool IntersectRays2D(float3x2 r1, float3x2 r2, out float3 intersection)
        {
            float4 r1_2D = new float4(r1[0].xz, r1[1].xz);
            float4 r2_2D = new float4(r2[0].xz, r2[1].xz);
            bool toReturn = IntersectRays2D(r1_2D, r2_2D, out float2 p);
            intersection = x0z(p);
            return toReturn;
        }

        public static bool IntersectSegmentToRay2D(
            in float3x2 firstSegmentRay,
            in float3x2 secondSegmentRay,
            in float3x2 ray,
            out float3x2 intersection)
        {
            intersection = new float3x2();

            bool first = IntersectRays2D(firstSegmentRay, ray, out intersection[0]);
            bool second = IntersectRays2D(secondSegmentRay, ray, out intersection[1]);
            return first && second;
        }

        public static bool IntersectSegmentRays(in float4x2 r1, in float4x2 r2, out float2x2 intersection)
        {
            intersection = new float2x2();
            // DrawRay(r1[0], 1, 100);
            // DrawRay(r2[0], 1, 100);
            bool first = IntersectRays2D(r1[0], r2[0], out intersection[0]);

            // DrawRay(r1[1], 1, 100);
            // DrawRay(r2[1], 1, 100);
            bool second = IntersectRays2D(r1[1], r2[1], out intersection[1]);
            return first && second;
        }

        public static void DrawRay(float4 ray, float length, float duration)
        {
            Debug.DrawRay(x0z(ray.xy), x0z(ray.zw) * length, Color.red, duration);
        }

        public static void DrawRay(float2 start, float2 dir, float length, float duration)
        {
            DrawRay(new float4(start, dir), length, duration);
        }
    }
}