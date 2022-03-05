using UnityEngine;

/// <summary>
/// Everything you need to know about level may go here.
/// </summary>
[System.Serializable]
public class LevelData
{
    public LevelData()
    {

    }
}

/// <summary>
/// It is expected that this script will present in every level to signify that a level was loaded.
/// </summary>
public class LevelDataHandOver : MonoBehaviour
{
    [SerializeField]
    private LevelData levelData;

    private void Awake()
    { 
        GeneralQueriesContainer.LevelData += GetLevelData;
        GeneralEventsContainer.LevelLoaded?.Invoke();
    }

    private void OnDestroy()
    {
        GeneralQueriesContainer.LevelData -= GetLevelData;
    }

    private LevelData GetLevelData()
    {
        return levelData;
    }
}
