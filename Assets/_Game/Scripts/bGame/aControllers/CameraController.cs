using UnityEngine;
using UnityEngine.Assertions;

public class CameraController : MonoBehaviour
{
    private Camera _renderingCamera;

    private void Awake()
    {
        bool isFound = TryGetComponent(out _renderingCamera);
        Assert.IsTrue(isFound);
        GameDelegatesContainer.GetRenderingCamera += GetRenderingCamera;
        InputDelegatesContainer.GetRenderingCamera += GetRenderingCamera;
    }

    private void OnDestroy()
    {
        GameDelegatesContainer.GetRenderingCamera -= GetRenderingCamera;
        InputDelegatesContainer.GetRenderingCamera -= GetRenderingCamera;
    }

    private Camera GetRenderingCamera()
    {
        return _renderingCamera;
    }
}
