using UnityEngine;
using UnityEngine.Assertions;

public class CameraController : MonoBehaviour
{
    private Camera _renderingCamera;

    private void Awake()
    {
        bool isFound = TryGetComponent(out _renderingCamera);
        Assert.IsTrue(isFound);

        GameDelegatesContainer.EventLevelStarted += OnLevelStarted;
    }

    private void OnDestroy()
    {
        GameDelegatesContainer.EventLevelStarted -= OnLevelStarted;
    }

    private void OnLevelStarted(LevelDescriptionSO notUsed)
    {
        InputDelegatesContainer.SetInputCamera(_renderingCamera);
    }

    private Camera GetRenderingCamera()
    {
        return _renderingCamera;
    }
}
