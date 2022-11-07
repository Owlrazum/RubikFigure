using System.Collections;
using UnityEngine;

public enum GameStateType
{
    MainMenu,
}

public class GameController : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField]
    private int _sceneIndexToTest = -1;
#endif

    [SerializeField]
    private GameDesciptionSO _gameDesc;

    private GameStateType _gameState;

    private int _currentLevel = 0;

    private void Awake()
    {
        GameDelegatesContainer.GetGameState += GetGameState;
        StandaloneInputDelegatesContainer.StartGameCommand += OnStartGameCommand;
        StandaloneInputDelegatesContainer.ExitToMainMenuCommand += OnExitToMainMenuCommand;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        GameDelegatesContainer.GetGameState -= GetGameState;
        StandaloneInputDelegatesContainer.StartGameCommand -= OnStartGameCommand;
        StandaloneInputDelegatesContainer.ExitToMainMenuCommand -= OnExitToMainMenuCommand;
    }

    private void Start()
    {
        if (_gameDesc == null)
        {
            return;
        }


#if UNITY_EDITOR
        Cursor.visible = true;
        Cursor.SetCursor(UnityEditor.PlayerSettings.defaultCursor, new Vector2(0.4f, 0), CursorMode.ForceSoftware);

        if (_sceneIndexToTest >= 0)
        {
            _currentLevel = _sceneIndexToTest;
            ApplicationDelegatesContainer.StartLoadingScene(_sceneIndexToTest);
        }
#endif
        ApplicationDelegatesContainer.StartLoadingScene(_gameDesc.Levels[_currentLevel].sceneIndex);
    }

    private GameStateType GetGameState()
    {
        return _gameState;
    }

    private void OnStartGameCommand()
    {
        // ApplicationDelegatesContainer.EventBeforeFinishingLoadingScene();
        ApplicationDelegatesContainer.FinishLoadingScene(OnStartGameLoadingSceneFinished);
    }

    private void OnStartGameLoadingSceneFinished()
    {
        GameDelegatesContainer.StartLevel(_gameDesc.Levels[_currentLevel]);

        GameDelegatesContainer.EventLevelStarted?.Invoke(_gameDesc.Levels[_currentLevel]);
    }

    private void OnExitToMainMenuCommand()
    {
        _gameState = GameStateType.MainMenu;
        ApplicationDelegatesContainer.LoadMainMenu(null);
    }
}