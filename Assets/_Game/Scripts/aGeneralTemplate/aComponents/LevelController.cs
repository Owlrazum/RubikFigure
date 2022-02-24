using UnityEngine;

public class LevelController : MonoBehaviour
{
    private void Awake()
    {
        //EventsContainer.PlayerReachedGates += CompleteLevel;
    }

    private void OnDestroy()
    { 
        //EventsContainer.PlayerReachedGates -= CompleteLevel;
    }

    private void CompleteLevel()
    {
        GeneralEventsContainer.LevelComplete?.Invoke();
    }
}