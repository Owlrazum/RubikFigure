// using UnityEngine;

// public class GameUtilsIntegration : MonoBehaviour
// {
//     private void Awake()
//     {
//         DontDestroyOnLoad(gameObject);

//         EventsContainer.LevelStart += OnLevelStart;
//         EventsContainer.LevelComplete += OnLevelComplete;
//     }

//     private void OnDestroy()
//     {
//         EventsContainer.LevelStart -= OnLevelStart;
//         EventsContainer.LevelComplete -= OnLevelComplete;
//     }

//     private void OnLevelStart(int levelIndex)
//     {
//         print("Calling GameUtils method");
//         YsoCorp.GameUtils.YCManager.instance.OnGameStarted(levelIndex);
//     }

//     private void OnLevelComplete(int levelIndex)
//     { 
//         print("Calling GameUtils method2");
//         YsoCorp.GameUtils.YCManager.instance.OnGameFinished(true);
//     }
// }
