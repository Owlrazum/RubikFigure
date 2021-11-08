using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    public class VibrationController : MonoBehaviour
    {
        [Header("Default Vibration")]
        [Space]
        [SerializeField]
        [Tooltip("In milliseconds")]
        private int vibrationTime = 200;

        [Header("Custom Vibration")]
        [Space]
        [SerializeField]
        [Tooltip("If not needed set it to zero.\n" +
        "Unity developer has no power to remove it...")]
        private int startDelay = 0;

        [Serializable]
        private struct VibrationData
        {
            public int vibrationTime;
            public int pauseTime;
        }

        [SerializeField]
        [Tooltip("Use it if there is a custom vibration with a pattern.\n" +
            "Vibr, pause, vibr, pause...")]
        private List<VibrationData> pattern;

        [SerializeField]
        private bool shouldRepeatPattern;

        private bool isAllowedToVibrate;
        public bool IsAllowedToVibrate
        {
            get
            {
                return isAllowedToVibrate;
            }
            set
            {
                isAllowedToVibrate = value;
                if (!isAllowedToVibrate)
                {
                    StopCuttingVibration();
                }
            }
        }

        public void Vibrate()
        {
            if (!IsAllowedToVibrate)
            {
                return;
            }
            Vibration.Vibrate(vibrationTime);
        }

        private long[] vibPattern;

        private void Awake()
        {
            Vibration.Init();

            vibPattern = new long[1 + pattern.Count * 2];
            vibPattern[0] = startDelay;

            int index = 1;
            for (int i = 0; i < pattern.Count; i++)
            {
                vibPattern[index++] = pattern[i].vibrationTime;
                vibPattern[index++] = pattern[i].pauseTime;
            }
        }

        private bool isVibrating;
        public void StartCuttingVibration()
        {
            if (!IsAllowedToVibrate)
            {
                return;
            }

            if (!isVibrating)
            {
                isVibrating = true;
                Vibration.Vibrate(vibPattern, shouldRepeatPattern ? 0 : -1);
            }
        }

        public void StopCuttingVibration()
        {
            if (!IsAllowedToVibrate)
            {
                return;
            }

            if (isVibrating)
            {

                StartCoroutine(OnNextFrame());

                Vibration.Cancel();
            }
        }

        private IEnumerator OnNextFrame()
        {
            yield return null;
            isVibrating = false;
        }
    }

}
