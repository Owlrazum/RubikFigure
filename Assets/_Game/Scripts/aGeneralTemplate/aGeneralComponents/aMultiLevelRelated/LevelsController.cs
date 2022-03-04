using UnityEngine;

public class LevelsController : MonoBehaviour
{
    private int currentLevelIndex;
    
    private bool areAllLevelsPassed;

    private const string pref_COMPLETED_LEVELS_COUNT = "CompletedLevelsCount";

    private void Awake()
    {
        currentLevelIndex = PlayerPrefs.GetInt(pref_COMPLETED_LEVELS_COUNT, 0);

        GeneralEventsContainer.LevelLoaded += StartLevel;
        //EventsContainer.PlayerReachedGates += CompleteLevel;
    }

    private void OnDestroy()
    { 
        GeneralEventsContainer.LevelLoaded -= StartLevel;
        //EventsContainer.PlayerReachedGates -= CompleteLevel;
    }

    private void StartLevel()
    {
        GeneralEventsContainer.LevelStart?.Invoke(currentLevelIndex);
    }

    private void CompleteLevel()
    {
        PlayerPrefs.SetInt(pref_COMPLETED_LEVELS_COUNT, currentLevelIndex);
        GeneralEventsContainer.LevelComplete?.Invoke(currentLevelIndex);
        currentLevelIndex++;
    }
}