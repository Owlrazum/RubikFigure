using UnityEngine;

public class UIMainMenuCanvas : MonoBehaviour
{
    private UIButton _startGameButton;
    private UIButton _exitGameButton;

    private void Awake()
    {
        transform.GetChild(0).TryGetComponent(out _startGameButton);
        _startGameButton.EventOnTouch += OnStartGameButtonPressed;

        if (transform.childCount >= 2)
        { 
            transform.GetChild(1).TryGetComponent(out _exitGameButton);
            if (_exitGameButton != null)
            { 
                _exitGameButton.EventOnTouch += OnExitGameButtonPressed;
            }
        }
    }

    private void OnDestroy()
    {
        _startGameButton.EventOnTouch -= OnStartGameButtonPressed;

        if (_exitGameButton != null)
        { 
            _exitGameButton.EventOnTouch -= OnExitGameButtonPressed;
        }
    }

    private void OnStartGameButtonPressed()
    {
        StandaloneInputDelegatesContainer.StartGameCommand?.Invoke();
    }

    private void OnExitGameButtonPressed()
    {
        if (StandaloneInputDelegatesContainer.ExitGameCommand != null)
        {
            StandaloneInputDelegatesContainer.ExitGameCommand.Invoke();
        }
        else
        { 
            Application.Quit(0);
        }
    }
}