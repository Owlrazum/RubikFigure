using UnityEngine;

/// <summary>
/// Needs Initialization;
/// </summary>
public class BeizerSegment : MonoBehaviour
{
    private CubicBeizerVector3Params par;

    public void Initialize()
    {
        if (transform.childCount != 4)
        {
            Debug.LogError("Incorrect child amount");
        }

        par = new CubicBeizerVector3Params();

        par.initial = transform.GetChild(0).position;
        par.anchor1 = transform.GetChild(1).position;
        par.anchor2 = transform.GetChild(2).position;
        par.target = transform.GetChild(3).position;
    }

    public Vector3 GetLerpedPos(float lerpParam)
    {
        Vector3 toReturn = CustomMath.ComputeCubicBeizerPos(par, lerpParam); ;
        return toReturn;
    }

    public Vector3 GetSegmentStart()
    {
        return par.initial;
    }

    public float GetApproximatedLength()
    {
        return Vector3.Distance(par.initial, par.target);
    }
}
