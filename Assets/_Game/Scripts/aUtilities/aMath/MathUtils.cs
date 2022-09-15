using Unity.Mathematics;

using UnityEngine;

namespace Orazum.Math
{
    public static class MathUtils
    {
        public const float Epsilon = 1E-5F;

        public static float3 x0z(in float2 xy)
        {
            return new float3(xy.x, 0, xy.y);
        }
        public static float3x2 x0z(in float2x2 xy)
        {
            return new float3x2(x0z(xy[0]), x0z(xy[1]));
        }

        // 2D cross product
        public static float Determinant(float2 lhs, float2 rhs)
        {
            return rhs.x * lhs.y - rhs.y * lhs.x;
        }

        public static bool Between01(in float lerpParam)
        {
            return lerpParam >= 0 && lerpParam <= 1;
        }

        public static void ClampToOne(ref float lerpParam)
        {
            if (lerpParam > 1)
            {
                lerpParam = 1;
            }
        }

        public static void ClampToZero(ref float lerpParam)
        {
            if (lerpParam < 0)
            {
                lerpParam = 0;
            }
        }

        public static Vector3 ProjectVector(Vector3 toProject, Vector3 onto)
        {
            Vector3 direction = onto.normalized;
            return Vector3.Dot(toProject, direction) * direction;
        }

        public static Vector3 ProjectPoint(Vector3 point, Vector3 lineStart, Vector3 lineEnd)
        {
            Vector3 onto = lineEnd - lineStart;
            Vector3 toProject = point - lineStart;
            Vector3 projection = ProjectVector(toProject, lineEnd - lineStart);
            return projection + lineStart;
        }

        public static float GetPointLineDistance(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            return Vector3.Cross((p0 - p1), (p0 - p2)).magnitude / (p2 - p1).magnitude;
        }

        public static Vector3 ComputeCubicBeizerPos(
            CubicBeizerVector3Params par,
            float lerpParam)
        {
            Vector3 secondOrderAnchor1 =
                Vector3.Lerp(par.initial, par.anchor1, lerpParam);
            Vector3 secondOrderAnchor2 =
                Vector3.Lerp(par.anchor1, par.anchor2, lerpParam);
            Vector3 secondOrderAnchor3 =
                Vector3.Lerp(par.anchor2, par.target, lerpParam);

            Vector3 thirdOrderAnchor1 =
                Vector3.Lerp(secondOrderAnchor1, secondOrderAnchor2, lerpParam);
            Vector3 thirdOrderAnchor2 =
                Vector3.Lerp(secondOrderAnchor2, secondOrderAnchor3, lerpParam);

            Vector3 cubicBeizerPos =
                Vector3.Lerp(thirdOrderAnchor1, thirdOrderAnchor2, lerpParam);

            return cubicBeizerPos;
        }

        public static float3 RotateAround(in float3 point, in float3 center, in quaternion q)
        {
            Debug.DrawRay(float3.zero, point, Color.green, 0.1f);
            return math.rotate(q, point - center) + center;
        }

        /// <summary>
        /// She would like it to be added to Unity.
        /// </summary>
        /// <see href="https://gist.github.com/FreyaHolmer/f7fdf72e9037f4cf7d10f232aedb97b0">Github page where she said it.</see>
        private static int t;
        #region FreyaHolmer
        
        public static float Frac(float x) => x - Mathf.Floor(x);
        public static float Smooth01(float x) => x * x * (3 - 2 * x);
        public static float InverseLerpUnclamped(float a, float b, float value) => (value - a) / (b - a);
        public static float Remap(float iMin, float iMax, float oMin, float oMax, float value)
        {
            float t = Mathf.InverseLerp(iMin, iMax, value);
            return Mathf.Lerp(oMin, oMax, t);
        }
        public static float RemapUnclamped(float iMin, float iMax, float oMin, float oMax, float value)
        {
            float t = InverseLerpUnclamped(iMin, iMax, value);
            return Mathf.LerpUnclamped(oMin, oMax, t);
        }

        // Vector2
        public static Vector2 AngToDir2D(float aRad) => new Vector2(Mathf.Cos(aRad), Mathf.Sin(aRad));
        public static float DirToAng2D(Vector2 dir) => Mathf.Atan2(dir.y, dir.x);
        public static Vector2 Rotate90CW2D(Vector2 v) => new Vector2(v.y, -v.x);
        public static Vector2 Rotate90CCW2D(Vector2 v) => new Vector2(-v.y, v.x);
        public static Vector2 Rotate2D(Vector2 v, float angRad)
        {
            float ca = Mathf.Cos(angRad);
            float sa = Mathf.Sin(angRad);
            return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
        }
        public static Vector2 RotateAround2D(Vector2 v, Vector2 pivot, float angRad) => Rotate2D(v - pivot, angRad) + pivot;
        #endregion
    }
}