using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using GeneralTemplate;
using UnityEngine.SceneManagement;

namespace GeneralTemplate
{
    public class UIController : MonoBehaviour
    {
        [SerializeField]
        private Canvas startGameCanvas;

        [Header("EndGameCanvas")]
        [Space]
        [SerializeField]
        private Canvas endGameCanvas;

        [SerializeField]
        private TextMeshProUGUI winText;

        [SerializeField]
        private TextMeshProUGUI defeatText;

        [SerializeField]
        private TextMeshProUGUI nextLevelText;

        [SerializeField]
        private TextMeshProUGUI repeatLevelText;

        [Header("SettingsCanvas")]
        [Space]
        [SerializeField]
        private Canvas settingsCanvas;

        [SerializeField]
        private Image soundOnImage;

        [SerializeField]
        private Image soundOffImage;

        [SerializeField]
        private Image hapticOnImage;

        [SerializeField]
        private Image hapticOffImage;

        [Header("...")]
        private float t;

        private void Start()
        {
            isHapticOn = true;
            isSoundOn = true;
        }

        public void ProcessGameEnd(GameResult result)
        {
            endGameCanvas.gameObject.SetActive(true);

            switch (result)
            {
                case GameResult.Win:
                    winText.gameObject.SetActive(true);
                    nextLevelText.gameObject.SetActive(true);
                    defeatText.gameObject.SetActive(false);
                    repeatLevelText.gameObject.SetActive(false);
                    break;
                case GameResult.Defeat:
                    defeatText.gameObject.SetActive(true);
                    repeatLevelText.gameObject.SetActive(true);
                    winText.gameObject.SetActive(false);
                    nextLevelText.gameObject.SetActive(false);
                    break;
            }
        }

        public void ProcessNextLevelButtonDown()
        {
            endGameCanvas.gameObject.SetActive(false);
            GameManager.Singleton.ProcessNextLevelButtonDown();

            winText.gameObject.SetActive(false);
        }

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

        private bool isHapticOn;
        public void ProcessHapticButtonDown()
        {
            GameManager.Singleton.AlternateHaptic();
            isHapticOn = !isHapticOn;
            if (isHapticOn)
            {
                hapticOnImage.gameObject.SetActive(true);
                hapticOffImage.gameObject.SetActive(false);
            }
            else
            {
                hapticOnImage.gameObject.SetActive(false);
                hapticOffImage.gameObject.SetActive(true);
            }
        }

        private bool isSoundOn;
        public void ProcessSoundButtonDown()
        {
            GameManager.Singleton.AlternateSound();
            isSoundOn = !isSoundOn;
            if (isSoundOn)
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
    }
}
