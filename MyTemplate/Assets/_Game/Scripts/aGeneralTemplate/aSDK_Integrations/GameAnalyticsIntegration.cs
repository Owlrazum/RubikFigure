//using UnityEngine;

//using GameAnalyticsSDK;


//namespace SDK_Integrations
//{
//    /// <summary>
//    /// Script execution order should come after GameAnalytics's own script.
//    /// </summary>
//    public class GameAnalyticsIntegration : MonoBehaviour
//    {
//        private void Awake()
//        {
//            GameAnalytics.Initialize();
//        }

//        public void ProcessStartLevel(int currentLevelIndex)
//        {
//            int startLevelIndexAdjusted = currentLevelIndex + 1;
//            GameAnalytics.NewProgressionEvent
//                (GAProgressionStatus.Start, "Level " + startLevelIndexAdjusted);
//        }

//        public void ProcessLevelComplete(int completedLevelIndex)
//        {
//            int completedLevelIndexAdjusted = completedLevelIndex + 1;
//            GameAnalytics.NewProgressionEvent
//                (GAProgressionStatus.Complete, "Level " + completedLevelIndexAdjusted);
//        }
//    }
//}

