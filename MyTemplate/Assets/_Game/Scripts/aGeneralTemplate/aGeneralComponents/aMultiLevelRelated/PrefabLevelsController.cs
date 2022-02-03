using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    public class PrefabLevelsController : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> levels;

        [SerializeField]
        private Transform levelsParent;

        [SerializeField]
        private bool shouldLoadSavedLevel = true;

        private int levelCount;
        private bool areAllLevelsPassed;
        private int currentLevelIndex;

        private GameObject loadingLevel;
        private GameObject currentLevel;

        private void Start()
        {
            levelCount = levels.Count;
            currentLevelIndex = 0;
        }

        public int GetCurrentLevelIndex()
        {
            return currentLevelIndex;
        }

        public void LoadSavedLevel(int levelIndexToTest = -1)
        {
            if (!shouldLoadSavedLevel)
            {
                return;
            }
            if (levelsParent.childCount > 0)
            {
                return;
            }
            int lastLevelIndex = PlayerPrefs.GetInt("LastLevel");
            if (levelIndexToTest >= 0)
            {
                lastLevelIndex = levelIndexToTest - 1;
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

// More simple version down here.

/*
 * [SerializeField]
        private List<GameObject> levels;

        [SerializeField]
        private Transform levelsParent;

        private int currentLevelIndex;

        private GameObject currentLevel;

        public string GetLevelName(int index)
        {
            return levels[index % levels.Count].name;
        }

        public void LoadLevel(int index)
        {
            if (currentLevel != null)
            {
                currentLevel.SetActive(false);
                Destroy(currentLevel);
            }
            currentLevel = Instantiate(levels[index % levels.Count], levelsParent);
            currentLevelIndex = index;
        }

        public void UnloadCurrentLevel()
        {
            if (currentLevel == null)
            {
                Debug.LogError("NoCurrentLevel to Unloads!");
                return;
            }

            Destroy(currentLevel);
        }

        public int GetCurrentLevelIndex()
        {
            return currentLevelIndex;
        }
 */
