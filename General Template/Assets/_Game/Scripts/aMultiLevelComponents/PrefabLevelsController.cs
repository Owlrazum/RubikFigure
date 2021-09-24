using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    public class PrefabLevelsController : MonoBehaviour
    {
        private int levelCount;
        private bool areAllLevelsPassed;
        private int currentLevelIndex;

        private GameObject loadingLevel;
        private GameObject currentLevel;

        [SerializeField]
        private List<GameObject> levels;

        [SerializeField]
        private Transform levelsParent;

        private void Start()
        {
            levelCount = levels.Count;
            currentLevelIndex = 0;
            if (levelsParent.transform.childCount != 1)
            {
                Debug.LogError("Current implementation supports only one child of levelsParent at startApp");
            }
            currentLevel = levelsParent.transform.GetChild(0).gameObject;
        }

        public void LoadSavedScene(int sceneIndexToTest = -1)
        {
            int lastLevelIndex = SaveSystem.GetInt("LastLevel");
            if (sceneIndexToTest >= 0)
            {
                lastLevelIndex = sceneIndexToTest;
            }
            currentLevel = Instantiate(levels[lastLevelIndex], levelsParent);
            currentLevelIndex = lastLevelIndex;
        }

        public void StartLoadingNextPrefab(
            int levelIndexToTest = -1)
        {
            int nextLevelIndex = currentLevelIndex + 1;
            print(currentLevelIndex + " " + nextLevelIndex);
            if (nextLevelIndex >= levelCount)
            {
                areAllLevelsPassed = true;
                print("ch2");
            }
            if (!areAllLevelsPassed)
            {
                if (levelIndexToTest >= 0)
                {
                    nextLevelIndex = levelIndexToTest;
                }
                loadingLevel = Instantiate(levels[nextLevelIndex], levelsParent);
                loadingLevel.SetActive(false);
                currentLevelIndex = nextLevelIndex;
                return;
            }
            nextLevelIndex = GetRandomValidLevelIndex();
            loadingLevel = Instantiate(levels[nextLevelIndex], levelsParent);
            loadingLevel.SetActive(false);
            currentLevelIndex = nextLevelIndex;
        }

        public void StartReloadingCurrentPrefab()
        {
            loadingLevel = Instantiate(levels[currentLevelIndex], levelsParent);
            loadingLevel.SetActive(false);
        }

        private int GetRandomValidLevelIndex()
        {
            int result = Random.Range(1, levelCount - 1);
            if (levelCount - 1 <= 1)
            {
                result = 0;
                print("ch");
            }
            if (result >= currentLevelIndex)
            {
                result++;
            }
            return result;
        }

        /// <summary>
        /// Should be called only when GameEnd is processed
        /// </summary>
        public void FinishLoadingLevel()
        {
            currentLevel.SetActive(false);
            loadingLevel.SetActive(true);
            Destroy(currentLevel);
            currentLevel = loadingLevel;
        }
    }
}
