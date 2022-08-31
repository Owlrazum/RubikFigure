using UnityEngine;
using Unity.Mathematics;
using static Orazum.Math.MathUtilities;

namespace Orazum.Utilities
{ 
    public static class DebugUtilities
    {
        public static void DrawGridCell(Vector3 pos, float rayLength, float time = -1)
        {
            Vector3 halfForward = rayLength / 2 * Vector3.forward;
            Vector3 halfRight = rayLength / 2 * Vector3.right;

            if (time < 0)
            { 
                Debug.DrawRay(pos - halfForward, rayLength * Vector3.forward, Color.red);
                Debug.DrawRay(pos - halfRight, rayLength * Vector3.right, Color.red);
            }
            else
            { 
                Debug.DrawRay(pos - halfForward, rayLength * Vector3.forward, Color.red, time);
                Debug.DrawRay(pos - halfRight, rayLength * Vector3.right, Color.red, time);
            }
        }

        public static void DrawQuad(float4x2 quad, float height, Color color, float duration)
        {
            Debug.DrawRay(x0z(quad[0].xy), Vector3.up * height, color, duration);
            Debug.DrawRay(x0z(quad[0].zw), Vector3.up * height, color, duration);
            Debug.DrawRay(x0z(quad[1].xy), Vector3.up * height, color, duration);
            Debug.DrawRay(x0z(quad[1].zw), Vector3.up * height, color, duration);
        }

        public static void DrawQuad(float2x2 s1, float2x2 s2, float height, Color color, float duration)
        {
            float4x2 quad = new float4x2(
                new float4(s1[0], s2[0]),
                new float4(s1[1], s2[1])
            );
            DrawQuad(quad, height, color, duration);
        }

        public static void DrawQuad(float3x4 quad, Color color, float duration)
        {
            Debug.DrawLine(quad[0], quad[1], color, duration);
            Debug.DrawLine(quad[1], quad[3], color, duration);
            Debug.DrawLine(quad[3], quad[2], color, duration);
            Debug.DrawLine(quad[2], quad[0], color, duration);
        }

        public static void DrawRay(float4 ray, float length, float duration)
        {
            Debug.DrawRay(x0z(ray.xy), x0z(ray.zw) * length, Color.white, duration);
        }

        public static void DrawRay(float2 start, float2 dir, float length, float duration)
        {
            DrawRay(new float4(start, dir), length, duration);
        }
    }
}