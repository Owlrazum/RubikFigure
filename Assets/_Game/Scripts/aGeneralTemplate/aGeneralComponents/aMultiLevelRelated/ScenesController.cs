using UnityEngine;
using UnityEngine.SceneManagement;

namespace GeneralTemplate
{
    public class ScenesController : MonoBehaviour
    {
        [SerializeField]
        private bool shouldLoadSavedLevel = true;

        private int sceneCount;
        private bool areAllLevelsPassed;

        private AsyncOperation loadingScene;

        private void Awake()
        {
            sceneCount = SceneManager.sceneCountInBuildSettings;

            GeneralEventsContainer.LevelComplete += StartLoadingNextScene;
            GeneralEventsContainer.ShouldLoadNextScene += FinishLoadingScene;
        }

        private void OnDestroy()
        { 
            GeneralEventsContainer.LevelComplete -= StartLoadingNextScene;
            GeneralEventsContainer.ShouldLoadNextScene -= FinishLoadingScene;
        }

        private void LoadSavedScene()
        {
            if (!shouldLoadSavedLevel)
            {
                return;
            }
            int lastLevel = PlayerPrefs.GetInt("LastLevel", 0);
            SceneManager.LoadScene(lastLevel);
        }

        private void StartLoadingNextScene()
        {
            if (!areAllLevelsPassed)
            { 
                int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
                if (nextSceneIndex >= sceneCount)
                {
                    areAllLevelsPassed = true;
                    StartLoadingRandomScene();
                    return;
                }
                loadingScene = SceneManager.LoadSceneAsync(nextSceneIndex);
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
            loadingScene.allowSceneActivation = false;
        }

        private int GetRandomValidSceneIndex()
        {
            if (sceneCount - 1 <= 1)
            {
                return 0;
            }

            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            return CustomUtility.RandomRangeWithExlusion(0, sceneCount, currentSceneIndex);
        }

        private void FinishLoadingScene()
        {
            loadingScene.allowSceneActivation = true;
        }

        private int GetCurrentLevelIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }
    }
}
