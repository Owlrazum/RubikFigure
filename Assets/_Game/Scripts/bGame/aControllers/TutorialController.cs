using UnityEngine;
using Orazum.Constants;

public class TutorialController : MonoBehaviour
{
    private bool shouldShowTutorial;

    private void Awake()
    { 
        shouldShowTutorial = PlayerPrefs.GetInt(PlayerPreferences.AtLeastOneLevelCompleted, 0) == 0;

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
            PlayerPrefs.SetInt(PlayerPreferences.AtLeastOneLevelCompleted, 1);
            shouldShowTutorial = false;
        }
    }

    private bool GetShouldShowTutorial()
    {
        return shouldShowTutorial;
    }
}