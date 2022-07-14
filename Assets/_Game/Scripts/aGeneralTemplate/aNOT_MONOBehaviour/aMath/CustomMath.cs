using UnityEngine;
using Unity.Mathematics;

[System.Serializable]
public struct CubicBeizerVector3Params
{
    public Vector3 initial;
    public Vector3 anchor1;
    public Vector3 anchor2;
    public Vector3 target;

    public override string ToString()
    {
        string toReturn =
            "initial " + initial + "\n" +
            "anchor1 " + anchor1 + "\n" +
            "anchor2 " + anchor2 + "\n" +
            "target  " + target + "\n";

        return toReturn;
    }
}

public static class CustomMath
{
    public const float Epsilon = 1E-12F;

    #region EasingFunctions
    public static float EaseIn(float lerpParam)
    {
        return lerpParam * lerpParam;
    }

    public static float Flip(float t)
    {
        return 1 - t;
    }

    public static float EaseOut(float lerpParam)
    {
        return Flip(EaseIn(Flip(lerpParam)));
    }

    public static float EaseInOut(float lerpParam)
    {
        return Mathf.Lerp(EaseIn(lerpParam), EaseOut(lerpParam), lerpParam);
    }
    #endregion

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

    public static float GetCrossProdSign(Vector3 lhs, Vector3 rhs)
    {
        return Mathf.Sign(Vector3.Cross(lhs, rhs).y);
    }

    public static float GetDotProdSign(Vector3 first, Vector3 second)
    {
        return Mathf.Sign(Vector3.Dot(first, second));
    }

    public static Vector3 ComputePointOnCircle(float radius, float radian)
    {
        Vector3 normalizedPoint =
            new Vector3(Mathf.Cos(radian), 0, Mathf.Sin(radian));
        return normalizedPoint * radius;
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

    public static bool AreQuaternionsEqual(Quaternion q1, Quaternion q2)
    {
        return Mathf.Abs(Quaternion.Dot(q1.normalized, q2.normalized)) > 1 - Epsilon;
    }

    /// <summary>
    /// Returns with
    /// x - min
    /// y - max
    /// </summary>
    public static float2 MinMaxComponentsRGB(float3 rgb)
    {
        float2 minMax = new float2(rgb.x, rgb.y);
        float x = rgb.x;
        float y = rgb.y;
        float z = rgb.z;
        if (x > y || x > z)
        {
            if (y < z)
            {
                minMax.x = y;
                if (x > z)
                {
                    minMax.y = x;
                }
                else
                {
                    minMax.y = z;
                }
            }
            else
            {
                minMax.x = z;
                if (x > y)
                {
                    minMax.y = x;
                }
                else
                {
                    minMax.y = y;
                }
            }
        }
        else
        { 
            if (y > z)
            {
                minMax.y = y;
            }
            else
            {
                minMax.y = z;
            }
        }

        return minMax;
    }

    public static float MaxComponentRGB(float3 rgb)
    {
        float x = rgb.x;
        float y = rgb.y;
        float z = rgb.z;
        if (x < y || x < z)
        {
            if (y < z)
            {
                return z;
            }
            else
            {
                return y;
            }
        }
        else
        {
            return x;
        }
    }

    public static float MinComponentRGB(float3 rgb)
    { 
        float x = rgb.x;
        float y = rgb.y;
        float z = rgb.z;
        if (x > y || x > z)
        {
            if (y > z)
            {
                return z;
            }
            else
            {
                return y;
            }
        }
        else
        {
            return x;
        }
    }

    /// <summary>
    /// She would like it to be added to Unity.
    /// </summary>
    /// <see href="https://gist.github.com/FreyaHolmer/f7fdf72e9037f4cf7d10f232aedb97b0">Github page where she said it.</see>
    #region FreyaHolmer
    // Mathf
    public const float TAU = 6.28318530717959f;
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
    public static Vector2 AngToDir(float aRad) => new Vector2(Mathf.Cos(aRad), Mathf.Sin(aRad));
    public static float DirToAng(Vector2 dir) => Mathf.Atan2(dir.y, dir.x);
    public static float Determinant/*or Cross*/(Vector2 a, Vector2 b) => a.x * b.y - a.y * b.x; // 2D "cross product"
    public static Vector2 Rotate90CW(Vector2 v) => new Vector2(v.y, -v.x);
    public static Vector2 Rotate90CCW(Vector2 v) => new Vector2(-v.y, v.x);
    public static Vector2 Rotate(Vector2 v, float angRad)
    {
        float ca = Mathf.Cos(angRad);
        float sa = Mathf.Sin(angRad);
        return new Vector2(ca * v.x - sa * v.y, sa * v.x + ca * v.y);
    }
    public static Vector2 RotateAround(Vector2 v, Vector2 pivot, float angRad) => Rotate(v - pivot, angRad) + pivot;

    // Vector2/3/4
    public static Vector2 Abs(Vector2 v) => new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    public static Vector3 Abs(Vector3 v) => new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    public static Vector4 Abs(Vector4 v) => new Vector4(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z), Mathf.Abs(v.w));
    public static Vector2 Round(Vector2 v) => new Vector2(Mathf.Round(v.x), Mathf.Round(v.y));
    public static Vector3 Round(Vector3 v) => new Vector3(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z));
    public static Vector4 Round(Vector4 v) => new Vector4(Mathf.Round(v.x), Mathf.Round(v.y), Mathf.Round(v.z), Mathf.Round(v.w));
    public static Vector2 Floor(Vector2 v) => new Vector2(Mathf.Floor(v.x), Mathf.Floor(v.y));
    public static Vector3 Floor(Vector3 v) => new Vector3(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z));
    public static Vector4 Floor(Vector4 v) => new Vector4(Mathf.Floor(v.x), Mathf.Floor(v.y), Mathf.Floor(v.z), Mathf.Floor(v.w));
    public static Vector2 Ceil(Vector2 v) => new Vector2(Mathf.Ceil(v.x), Mathf.Ceil(v.y));
    public static Vector3 Ceil(Vector3 v) => new Vector3(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z));
    public static Vector4 Ceil(Vector4 v) => new Vector4(Mathf.Ceil(v.x), Mathf.Ceil(v.y), Mathf.Ceil(v.z), Mathf.Ceil(v.w));
    #endregion
}