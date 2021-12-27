using UnityEngine;

public class CustomUtility : MonoBehaviour
{
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
