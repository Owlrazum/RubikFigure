using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace GeneralTemplate
{
    /// <summary>
    /// Base functionality of UIController.
    /// There are children of this class, in UIControllerVariations folder.
    /// Derive and override necessary methods if the game is a usual hyperCash.
    /// Write your own class in case the game is unusual or not a hyperCash.
    /// TODO: make classes for each canvas, so serialized field reassigning
    /// will be minimised. Or just make prefab variants.
    /// 
    /// </summary>
    public class UIControllerBase : MonoBehaviour
    {
        [SerializeField]
        private Canvas startGameCanvas;

        [Header("EndLevelCanvas")]
        [Space]
        [SerializeField]
        protected Canvas endLevelCanvas;

        [SerializeField]
        protected Animator endLevelCanvasAnimator;

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

        private void Start()
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

        // ====== Overridables ======

        public virtual void ProcessLevelEnd()
        {
            endLevelCanvas.gameObject.SetActive(true);

            endLevelCanvasAnimator.Play("Appear");
        }

        public virtual void ProcessNextLevelButtonDown()
        {
            endLevelCanvas.gameObject.SetActive(false);
            //GameManager.Singleton.ProcessNextLevelButtonDown();
        }

        // May be used to process scenarios where either win or defeat can happen.
        public virtual void ProcessGameEnd(GameResult result)
        {

        }

        // ====== End Section ======


        // It is likely you would not need to modify this region

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
