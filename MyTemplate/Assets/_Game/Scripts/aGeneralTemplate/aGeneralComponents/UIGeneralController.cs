using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace GeneralTemplate
{
    /// <summary>
    /// UI Controller responsible for common functionality from the General Template. 
    /// For custom behaviour create separate canvases, which are likely to inherit from
    /// UIBaseFadingCanvas.
    /// </summary>
    public class UIGeneralController : MonoBehaviour
    {
        // see CommonFunctionality comment
        #region CommonSerializedFields
        [Header("SettingsCanvas")]
        [Space]
        [SerializeField]
        private Canvas settingsCanvas;

        [SerializeField]
        private Image soundOnImage;

        [SerializeField]
        private Image soundOffImage;

        [SerializeField]
        private Image vibrateOnImage;

        [SerializeField]
        private Image vibrateOffImage;
        #endregion 

        #region BuildDebugging
        [Header("BuildDebugging")]
        [Space]
        [SerializeField]
        private bool shouldLogInBuild;

        [SerializeField]
        private TextMeshProUGUI logBuildText;

        [SerializeField]
        private int logLimit;

        private void Awake()
        {
            shouldPresentVibrationOn = true;
            shouldPresentSoundOn = true;

            logAmount = 0;
        }

        private int logAmount;
        public void DebugLogBuild(string log)
        {
            if (!shouldLogInBuild)
            {
                return;
            }
            logAmount++;
            if (logAmount > logLimit)
            {
                logBuildText.text = "";
                logAmount = 0;
            }
            logBuildText.text += " " + log;
        }
        #endregion

        #region CommonFunctionality

        private bool isShowingSettings;
        public void ProcessSettingsButtonDown()
        {
            isShowingSettings = !isShowingSettings;
            if (isShowingSettings)
            {
                settingsCanvas.gameObject.SetActive(true);
            }
            else
            {
                settingsCanvas.gameObject.SetActive(false);
            }
        }

        public void ProcessOrdinaryTapInSettings()
        {
            settingsCanvas.gameObject.SetActive(false);
        }

        // =========   Sound   =========

        private bool shouldPresentSoundOn;
        public void ProcessSoundButtonDown()
        {
            GameManager.Singleton.AlternateSound();
            shouldPresentSoundOn = !shouldPresentSoundOn;
            UpdateSoundUI();
        }

        private void UpdateSoundUI()
        {
            if (shouldPresentSoundOn)
            {
                soundOnImage.gameObject.SetActive(true);
                soundOffImage.gameObject.SetActive(false);
            }
            else
            {
                soundOnImage.gameObject.SetActive(false);
                soundOffImage.gameObject.SetActive(true);
            }
        }

        public void SetSoundPresentation(bool shouldPresentSoundOnArg)
        {
            shouldPresentSoundOn = shouldPresentSoundOnArg;
            UpdateSoundUI();
        }

        // =========   EndSound   =========


        // =========   Vibration   =========

        private bool shouldPresentVibrationOn;
        public void ProcessVibrationButtonDown()
        {
            GameManager.Singleton.AlternateHaptic();
            shouldPresentVibrationOn = !shouldPresentVibrationOn;
            UpdateVibrationUI();
        }

        private void UpdateVibrationUI()
        {
            if (shouldPresentVibrationOn)
            {
                vibrateOnImage.gameObject.SetActive(true);
                vibrateOffImage.gameObject.SetActive(false);
            }
            else
            {
                vibrateOnImage.gameObject.SetActive(false);
                vibrateOffImage.gameObject.SetActive(true);
            }
        }

        public void SetVibrationPresentation(bool shouldPresentHapticOnArg)
        {
            shouldPresentVibrationOn = shouldPresentHapticOnArg;
            UpdateVibrationUI();
        }

        // =========   EndHaptic   =========
        #endregion
    }
}
