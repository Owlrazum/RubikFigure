using UnityEngine;
using Orazum.Utilities.ConstContainers;

public class TutorialController : MonoBehaviour
{
    private bool shouldShowTutorial;

    private void Awake()
    { 
        shouldShowTutorial = PlayerPrefs.GetInt(PlayerPrefsContainer.AtLeastOneLevelCompleted, 0) == 0;

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
            PlayerPrefs.SetInt(PlayerPrefsContainer.AtLeastOneLevelCompleted, 1);
            shouldShowTutorial = false;
        }
    }

    private bool GetShouldShowTutorial()
    {
        return shouldShowTutorial;
    }
}