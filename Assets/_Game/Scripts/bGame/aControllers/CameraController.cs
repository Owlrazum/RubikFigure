using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Camera _renderingCamera;

    private void Awake()
    {
    }

    private void OnDestroy()
    {
    }
}
