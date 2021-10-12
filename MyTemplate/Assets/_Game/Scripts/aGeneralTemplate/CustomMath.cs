using UnityEngine;

public static class CustomMath
{
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
}