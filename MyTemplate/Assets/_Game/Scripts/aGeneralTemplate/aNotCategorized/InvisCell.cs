using UnityEngine;

/// <summary>
/// It is referenced from the Object Poool
/// </summary>
public class InvisCell : MonoBehaviour
{
    [SerializeField]
    private Transform toScale;

    public void OnSpawn(Vector3 worldPos)
    {
        transform.position = worldPos;
        gameObject.SetActive(true);
    }

    public void OnDespawn()
    {
        gameObject.SetActive(false);
    }

    public Transform GetMovableTransform()
    {
        return transform;
    }

    public Transform GetScalableTransform()
    {
        return toScale;
    }

    public void IncreaseScaleToContainPos(Vector3 worldPos)
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
        transform.position += posDelta / 2;
    }

    public void DecreaseScaleToExcludePos(Vector3 worldPos)
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
        transform.position += posDelta / 2;
    }
}
