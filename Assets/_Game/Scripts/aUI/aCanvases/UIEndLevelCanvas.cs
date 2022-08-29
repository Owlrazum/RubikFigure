using UnityEngine;

public class UIEndLevelCanvas : UIBaseFadingCanvas
{
    [SerializeField]
    private UIButton _returnToMainMenuButton;

    protected override void Awake()
    {
        base.Awake();

        _canvasGroup.alpha = 0;

        UIDelegatesContainer.ShowEndLevelCanvas += ShowItself;
        _returnToMainMenuButton.EventOnTouch += OnReturnMainMenuPressed;
    }

    private void OnDestroy()
    { 
        UIDelegatesContainer.ShowEndLevelCanvas -= ShowItself;
        _returnToMainMenuButton.EventOnTouch -= OnReturnMainMenuPressed;
    }

    private void OnReturnMainMenuPressed()
    {
        StandaloneInputDelegatesContainer.ExitToMainMenuCommand();
    }
}