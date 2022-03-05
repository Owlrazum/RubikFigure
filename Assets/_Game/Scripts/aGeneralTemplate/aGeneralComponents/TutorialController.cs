using UnityEngine;

public class TutorialController : MonoBehaviour
{
    private bool shouldShowTutorial;

    private const string pref_AT_LEAST_ONE_LEVEL_COMPLETED = "AtLeastOneLevelCompleted";

    private void Awake()
    { 
        shouldShowTutorial = PlayerPrefs.GetInt(pref_AT_LEAST_ONE_LEVEL_COMPLETED, 0) == 0;

        GeneralEventsContainer.LevelComplete += OnLevelComplete;

        GeneralQueriesContainer.ShouldShowTutorial += GetShouldShowTutorial;
    }

    private void OnDestroy()
    { 
        GeneralEventsContainer.LevelComplete -= OnLevelComplete;

        GeneralQueriesContainer.ShouldShowTutorial -= GetShouldShowTutorial;
    }

    private void OnLevelComplete(int notUsed)
    { 
        if (shouldShowTutorial)
        { 
            PlayerPrefs.SetInt(pref_AT_LEAST_ONE_LEVEL_COMPLETED, 1);
            shouldShowTutorial = false;
        }
    }

    private bool GetShouldShowTutorial()
    {
        return shouldShowTutorial;
    }
}