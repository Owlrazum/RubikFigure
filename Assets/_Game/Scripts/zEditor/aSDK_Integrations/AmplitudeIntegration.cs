//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//namespace SDK_Integrations
//{
//    public class AbaiAmplitude : MonoBehaviour
//    {
//        public static AbaiAmplitude Singleton;
//        private void Awake()
//        {
//            if (Singleton == null)
//            {
//                Singleton = this;
//                InitializeAmplitude();
//            }
//        }

//        private Dictionary<string, object> gameStartProperty;
//        private Dictionary<string, object> levelStartProperty;
//        private Dictionary<string, object> levelCompleteProperty;
//        private Dictionary<string, object> failProperty;
//        private Dictionary<string, object> restartProperty;

//        private void InitializeAmplitude()
//        {

//            DO NOT DELETE BEFORE READING!!!  

//            amplitude.init accepts specific id, should be created and pasted manually

//            Amplitude amplitude = Amplitude.Instance;
//            amplitude.setServerUrl("https://api2.amplitude.com");
//            amplitude.logging = true;
//            amplitude.trackSessionEvents(true);
//            amplitude.init("2155d212af068f3247767c80c4577cdc");
//        }

//        public void LogGameStart()
//        {
//            UpdateGameStartProperty();
//            Amplitude.Instance.logEvent("game_start", gameStartProperty);
//        }

//        public void LogLevelStart(int levelIndex)
//        {
//            UpdateLevelStartProperty(levelIndex);
//            Amplitude.Instance.logEvent("level_start", levelStartProperty);
//        }

//        public void LogLevelComplete(int levelIndex, int secondsSpent)
//        {
//            UpdateLevelCompleteProperty(levelIndex, secondsSpent);
//            Amplitude.Instance.logEvent("level_complete", levelCompleteProperty);
//        }

//        public void LogFail(int levelIndex, int secondsSpent)
//        {
//            UpdateFailProperty(levelIndex, secondsSpent);
//            Amplitude.Instance.logEvent("fail", failProperty);
//        }

//        public void LogRestart(int levelIndex)
//        {
//            UpdateRestartProperty(levelIndex);
//            Amplitude.Instance.logEvent("restart", restartProperty);
//        }

//        /// <summary>
//        /// Should be settings?
//        /// </summary>
//        public void LogMainMenu()
//        {
//            Amplitude.Instance.logEvent("main_menu");
//        }

//        private void UpdateGameStartProperty()
//        {
//            gameStartProperty = new Dictionary<string, object>(1);
//            int count = PlayerPrefs.GetInt("gameStart_Count", 0);
//            count++;
//            gameStartProperty.Add("count", count);
//            PlayerPrefs.SetInt("gameStart_Count", count);
//        }

//        private void UpdateLevelStartProperty(int levelIndex)
//        {
//            levelStartProperty = new Dictionary<string, object>(1);
//            levelStartProperty.Add("level", levelIndex);
//        }

//        private void UpdateLevelCompleteProperty(int levelIndex, int secondsSpent)
//        {
//            levelCompleteProperty = new Dictionary<string, object>(2);
//            levelCompleteProperty.Add("level", levelIndex);
//            levelCompleteProperty.Add("time_spent", secondsSpent);
//        }

//        private void UpdateFailProperty(int levelIndex, int secondsSpent)
//        {
//            failProperty = new Dictionary<string, object>(2);
//            failProperty.Add("level", levelIndex);
//            failProperty.Add("time_spent", secondsSpent);
//        }

//        private void UpdateRestartProperty(int levelIndex)
//        {
//            restartProperty = new Dictionary<string, object>(1);
//            restartProperty.Add("level", levelIndex);
//        }

//        public void LogCustomEvent(string nameOfEvent)
//        {
//            Amplitude.Instance.logEvent(nameOfEvent);
//        }
//    }

//}
