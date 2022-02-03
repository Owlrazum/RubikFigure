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

        private void Start()
        {
            sceneCount = SceneManager.sceneCountInBuildSettings;
        }
        
        public void LoadSavedScene(int sceneIndexToTest = -1)
        {
            if (!shouldLoadSavedLevel)
            {
                return;
            }
            int lastLevel = PlayerPrefs.GetInt("LastLevel");
            if (sceneIndexToTest >= 0)
            {
                lastLevel = sceneIndexToTest;
            }
            SceneManager.LoadScene(lastLevel);
        }

        public void StartLoadingNextScene(int sceneIndexToTest = -1)
        {
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            if (nextSceneIndex >= sceneCount)
            {
                areAllLevelsPassed = true;
            }
            if (!areAllLevelsPassed)
            {
                if (sceneIndexToTest >= 0)
                {
                    nextSceneIndex = sceneIndexToTest;
                }
                loadingScene = SceneManager.LoadSceneAsync(nextSceneIndex);
                loadingScene.allowSceneActivation = false;
                return;
            }
            nextSceneIndex = GetRandomValidSceneIndex();
            loadingScene = SceneManager.LoadSceneAsync(nextSceneIndex);
            loadingScene.allowSceneActivation = false;
        }

        private int GetRandomValidSceneIndex()
        {
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            int result = Random.Range(1, sceneCount - 1);
            if (sceneCount - 1 <= 1)
            {
                result = 0;
            }
            if (result >= currentSceneIndex)
            {
                result++;
            }
            return result;
        }

        public void StartReloadingCurrentScene(GameObject multiSceneParent)
        {
            if (SceneManager.GetActiveScene().buildIndex == 0)
            {
                PrepareForReloadOfFirstScene(multiSceneParent);
            }
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            loadingScene = SceneManager.LoadSceneAsync(currentSceneIndex);
            loadingScene.allowSceneActivation = false;
            return;
        }

        private void PrepareForReloadOfFirstScene(GameObject multiSceneParent)
        {
            SceneManager.MoveGameObjectToScene(
                multiSceneParent,
                SceneManager.GetActiveScene()
            );
        }

        /// <summary>
        /// Should be called only when GameEnd is processed
        /// </summary>
        public void FinishLoadingScene()
        {
            loadingScene.allowSceneActivation = true;
        }

        public int GetCurrentLevelIndex()
        {
            return SceneManager.GetActiveScene().buildIndex;
        }
    }
}
