using UnityEngine;

public class TutorialController : MonoBehaviour
{
    private bool shouldShowTutorial;

    private const string pref_AT_LEAST_ONE_LEVEL_COMPLETED = "AtLeastOneLevelCompleted";

    private void Awake()
    { 
        shouldShowTutorial = PlayerPrefs.GetInt(pref_AT_LEAST_ONE_LEVEL_COMPLETED, 0) == 0;

        GameDelegatesContainer.EventLevelCompleted += OnLevelCompleted;

        GameDelegatesContainer.GetShouldShowTutorial += GetShouldShowTutorial;
    }

    private void OnDestroy()
    { 
        GameDelegatesContainer.EventLevelCompleted -= OnLevelCompleted;

        GameDelegatesContainer.GetShouldShowTutorial -= GetShouldShowTutorial;
    }

    private void OnLevelCompleted()
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