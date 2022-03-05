using UnityEngine;
using UnityEngine.SceneManagement;

namespace GeneralTemplate
{
    public class ScenesController : MonoBehaviour
    {
        [SerializeField]
        private int sceneToTest = -1;

        private int sceneCount;
        private AsyncOperation loadingScene;

        private const string pref_LAST_SCENE_LEVEL_INDEX = "LastSceneLevelIndex";

        private void Awake()
        {
            sceneCount = SceneManager.sceneCountInBuildSettings;

            GeneralEventsContainer.GameStart += LoadSavedScene;
            GeneralEventsContainer.LevelComplete += StartLoadingNextScene;
            GeneralEventsContainer.ShouldLoadNextSceneLevel += FinishLoadingSceneLevel;
        }

        private void OnDestroy()
        {
            GeneralEventsContainer.GameStart -= LoadSavedScene;
            GeneralEventsContainer.LevelComplete -= StartLoadingNextScene;
            GeneralEventsContainer.ShouldLoadNextSceneLevel -= FinishLoadingSceneLevel;
        }

        private void LoadSavedScene()
        {
            if (sceneToTest > 0)
            { 
                SceneManager.LoadScene(sceneToTest);
                return;
            }
            int lastLevel = PlayerPrefs.GetInt(pref_LAST_SCENE_LEVEL_INDEX, 1);
            SceneManager.LoadScene(lastLevel);
        }

        private void StartLoadingNextScene(int useless)
        {
            if (!GeneralQueriesContainer.QueryAreAllLevelsPassed())
            { 
                int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
                if (nextSceneIndex >= sceneCount)
                {
                    GeneralEventsContainer.AllLevelsWerePassed?.Invoke();
                    StartLoadingRandomScene();
                    return;
                }
                loadingScene = SceneManager.LoadSceneAsync(nextSceneIndex);
                PlayerPrefs.SetInt(pref_LAST_SCENE_LEVEL_INDEX, nextSceneIndex);
                loadingScene.allowSceneActivation = false;
            }
            else
            {
                StartLoadingRandomScene();
            }
        }

        private void StartLoadingRandomScene()
        { 
            int nextSceneIndex = GetRandomValidSceneIndex();
            loadingScene = SceneManager.LoadSceneAsync(nextSceneIndex);
            PlayerPrefs.SetInt(pref_LAST_SCENE_LEVEL_INDEX, nextSceneIndex);
            loadingScene.allowSceneActivation = false;
        }

        private int GetRandomValidSceneIndex()
        {
            if (sceneCount - 1 <= 1)
            {
                return 1;
            }
            if (sceneToTest > 0)
            {
                return sceneToTest;
            }

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            return CustomUtility.RandomRangeWithExlusion(1, sceneCount, currentSceneIndex);
        }

        private void FinishLoadingSceneLevel()
        {
            loadingScene.allowSceneActivation = true;
        }
    }
}
