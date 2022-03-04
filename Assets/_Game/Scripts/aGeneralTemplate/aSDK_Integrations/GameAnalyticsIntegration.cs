// using UnityEngine;

// using GameAnalyticsSDK;

// namespace SDK_Integrations
// {
//     /// <summary>
//     /// Script execution order should come after GameAnalytics's own script.
//     /// </summary>
//     public class GameAnalyticsIntegration : MonoBehaviour
//     {
//         private void Awake()
//         {
//             GameAnalytics.Initialize();

//             GeneralEventsContainer.LevelStart += OnLevelStart;
//             GeneralEventsContainer.LevelComplete += OnLevelComplete;
            
//             DontDestroyOnLoad(gameObject);
//         }

//         private void OnDestroy()
//         {
//             GeneralEventsContainer.LevelStart -= OnLevelStart;
//             GeneralEventsContainer.LevelComplete -= OnLevelComplete;
//         }

//         private void OnLevelStart(int currentLevelIndex)
//         {
//             int startLevelIndexAdjusted = currentLevelIndex + 1;
//             GameAnalytics.NewProgressionEvent
//                 (GAProgressionStatus.Start, "Level " + startLevelIndexAdjusted);
//         }

//         private void OnLevelComplete(int completedLevelIndex)
//         {
//             int completedLevelIndexAdjusted = completedLevelIndex + 1;
//             GameAnalytics.NewProgressionEvent
//                 (GAProgressionStatus.Complete, "Level " + completedLevelIndexAdjusted);
//         }
//     }
// }

