using UnityEngine;

public class CustomUtility
{
    /// <summary>
    /// Get random integer within range that is not equal to the toExclude
    /// </summary>
    public static int RandomRangeWithExlusion(int start, int end, int toExclude)
    {
        int result = Random.Range(start, end - 1);
        if (result >= toExclude)
        {
            result++;
        }
        return result;
    }

    public (Transform toMove, Transform toScale) IncreaseScaleToContainPos
           (Transform toMove, Transform toScale, Vector3 worldPos)
    {
        Vector3 pos = toScale.InverseTransformPoint(worldPos);
        Vector3 delta = Vector3.zero;

        Vector3 initialScale = toScale.localScale;
        Vector3 scaleDelta = Vector3.zero;

        if (Mathf.Abs(pos.x) > 0.5f)
        {
            float border = pos.x < 0 ? -0.5f : 0.5f;
            float d = pos.x - border;
            delta.x = d;

            scaleDelta.x = Mathf.Abs(delta.x * initialScale.x);
        }

        if (Mathf.Abs(pos.y) > 0.5f)
        {
            float border = pos.y < 0 ? -0.5f : 0.5f;
            float d = pos.y - border;
            delta.y = d;

            scaleDelta.y = Mathf.Abs(delta.y * initialScale.y);
        }

        if (Mathf.Abs(pos.z) > 0.5f)
        {
            float border = pos.z < 0 ? -0.5f : 0.5f;
            float d = pos.z - border;
            delta.z = d;

            scaleDelta.z = Mathf.Abs(delta.z * initialScale.z);
        }

        toScale.localScale += scaleDelta;
        Vector3 posDelta = toScale.TransformVector(delta);
        toMove.position += posDelta / 2;
        return (toMove, toScale);
    }

    public (Transform toMove, Transform toScale) DecreaseScaleToExcludePos
           (Transform toMove, Transform toScale, Vector3 worldPos)
    {
        Vector3 pos = toScale.InverseTransformPoint(worldPos);
        Vector3 delta = Vector3.zero;

        Vector3 initialScale = toScale.localScale;
        Vector3 scaleDelta = Vector3.zero;

        if (Mathf.Abs(pos.x) < 0.5f)
        {
            float border = pos.x < 0 ? -0.5f : 0.5f;
            float d = pos.x - border;
            delta.x = d;

            scaleDelta.x = Mathf.Abs(delta.x * initialScale.x);
        }

        if (Mathf.Abs(pos.y) < 0.5f)
        {
            float border = pos.y < 0 ? -0.5f : 0.5f;
            float d = pos.y - border;
            delta.y = d;

            scaleDelta.y = Mathf.Abs(delta.y * initialScale.y);
        }

        if (Mathf.Abs(pos.z) < 0.5f)
        {
            float border = pos.z < 0 ? -0.5f : 0.5f;
            float d = pos.z - border;
            delta.z = d;

            scaleDelta.z = Mathf.Abs(delta.z * initialScale.z);
        }

        toScale.localScale -= scaleDelta;
        Vector3 posDelta = toScale.TransformVector(delta);
        toMove.position += posDelta / 2;
        return (toMove, toScale);
    }

    /// <summary>
    /// Uses box colliders to fit object to the place
    /// </summary>
    public static Vector3 GetScaleToFitTransformToTheTargetBox(Transform origin, Transform target)
    {
        if (!origin.TryGetComponent(out BoxCollider originBox))
        {
            originBox = origin.gameObject.AddComponent<BoxCollider>();
        }

        if (!target.TryGetComponent(out BoxCollider targetBox))
        {
            targetBox = origin.gameObject.AddComponent<BoxCollider>();
        }

        Vector3 originSize = CustomMath.Abs(origin.TransformVector(originBox.size));
        Vector3 targetSize = CustomMath.Abs(target.TransformVector(targetBox.size));

        Vector3 initialScale = origin.localScale;
        Vector3 newScale = origin.localScale;
        Vector3 scaleFactor = Vector3.one;

        float deltaX = targetSize.x - originSize.x;

        if (deltaX < 0)
        {
            float factor = targetSize.x / originSize.x;
            scaleFactor.x = factor;
            newScale *= factor;
            origin.localScale = newScale;
        }

        originSize = CustomMath.Abs(origin.TransformVector(originBox.size));
        float deltaY = targetSize.y - originSize.y;

        if (deltaY < 0)
        {
            float factor = targetSize.y / originSize.y;
            scaleFactor.y = factor;
            newScale *= factor;
            origin.localScale = newScale;
        }

        originSize = CustomMath.Abs(origin.TransformVector(originBox.size));
        float deltaZ = targetSize.z - originSize.z;
        if (deltaZ < 0)
        {
            float factor = targetSize.z / originSize.z;
            scaleFactor.z = factor;
            newScale *= factor;
        }

        origin.localScale = initialScale;

        newScale = origin.localScale;
        newScale *= scaleFactor.x;
        newScale *= scaleFactor.y;
        newScale *= scaleFactor.z;

        return newScale;
    }
}
