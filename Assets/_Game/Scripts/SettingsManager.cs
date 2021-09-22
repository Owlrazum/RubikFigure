using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using MoreMountains.NiceVibrations;
using Sirenix.OdinInspector;
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Singleton;
    [BoxGroup]
    public Settings settings;
    //[BoxGroup("���� �������", centerLabel: true)]
    [SerializeField] Panels panels;
    //[BoxGroup("��������� �����", centerLabel: true)]
    [SerializeField] Money money;
    //[BoxGroup("��������� �������", centerLabel: true)]
    [SerializeField] LevelManager lvlManager;
    [Header("Sounds")]
    [Tooltip("������� AudioSource ������� ����� ����������� ������ � �����")]
    [SerializeField] AudioSource audioSource;
    [BoxGroup("�������� ����� ������ ����", centerLabel: true)]
    [Space]
    public Sounds audioSounds;
    void Awake()
    {
        Singleton = this;
        lvlManager.StartLevel();
    }
    
    #region Hided
    private void Start()
    {
        settings.Audio.AudioManager = audioSource;
        settings.Audio.StartSound();
        settings.Vibration.StartVibration();
        money.StartMoney();
    }
    public void SoundSet()
    {
        settings.Audio.IsAudio = !settings.Audio.IsAudio;
    }
    public void VibrationSet()
    {
        settings.Vibration.IsVibration = !settings.Vibration.IsVibration;
    }
    public void NextLevel()
    {
        WinLvl();
        lvlManager.NextLevel();
    }
    public void RestartLevel() {
        LoseLvl();
        lvlManager.RestartLevel();
    }
    public void WinLvl()
    {
        panels.WinGame();
    }
    public void LoseLvl()
    {
        panels.LoseGame();
    }
    public void PauseResumeGame()
    {
        panels.PausePanelSet();
    }
    public void AddMoney(int value)
    {
        money.AddMoney(value);
    }
    public int GetMoney()
    {
        return money.GetMoney();
    }
    #endregion
    #region Classes
    [Serializable]
    public class Panels
    {
        [Tooltip("UI ������ ���� �����")]
        [SerializeField] GameObject PausePanel;
        [Tooltip("UI ������ ���� ������")]
        [SerializeField] GameObject WinPanel;
        [Tooltip("UI ������ ���� ���������")]
        [SerializeField] GameObject LosePanel;
        private float timeScale;
        public void WinGame()
        {
            WinPanel.SetActive(!WinPanel.activeSelf);
        }
        public void LoseGame()
        {
            LosePanel.SetActive(!LosePanel.activeSelf);
        }
        public void PausePanelSet()
        {
            PausePanel.SetActive(!PausePanel.activeSelf);
            if (PausePanel.activeSelf)
            {
                timeScale = Time.timeScale;
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = timeScale;
            }
        }
    }
    [System.Serializable]
    public class Settings
    {
        [BoxGroup("��������� ������",centerLabel:true)]
        public AudioSettings Audio;
        [BoxGroup("��������� ��������", centerLabel: true)]
        public VibrationSettings Vibration;
        [System.Serializable]
        public class AudioSettings
        {
            [Tooltip("UI ������-������ �����")]
            [SerializeField] Image SoundIconRenderer;
            [Tooltip("������ ����������� �����")]
            [SerializeField] Sprite SoundOn;
            [Tooltip("������ ������������ �����")]
            [SerializeField] Sprite SoundOff;
            [HideInInspector]
            public AudioSource AudioManager;
            private bool hasAudio;
            public bool IsAudio
            {
                get { return hasAudio; }
                set
                {
                    hasAudio = value;
                    ChangeSounnd(hasAudio);
                }
            }
            public void PlaySound(AudioClip value)
            {
                AudioManager.PlayOneShot(value);
            }
            public void StartSound()
            {
                float temp = PlayerPrefs.GetFloat("AudioSet", 1);
                if (temp == 0)
                {
                    hasAudio = false;
                    AudioListener.volume = 0;
                    SoundIconRenderer.sprite = SoundOff;
                }
                else
                {
                    hasAudio = true;
                    AudioListener.volume = 1;
                    SoundIconRenderer.sprite = SoundOn;
                }
            }
            public void ChangeSounnd(bool value)
            {
                if (value)
                {
                    SoundIconRenderer.sprite = SoundOn;

                    AudioListener.volume = 1;
                    PlayerPrefs.SetFloat("AudioSet", AudioListener.volume);
                }
                else
                {
                    SoundIconRenderer.sprite = SoundOff;

                    AudioListener.volume = 0;
                    PlayerPrefs.SetFloat("AudioSet", AudioListener.volume);
                }
            }
        }
        [System.Serializable]
        public class VibrationSettings
        {
            [Tooltip("UI ������-������ ��������")]
            [SerializeField] Image VibrationIconRenderer;
            [Tooltip("������ ���������� ��������")]
            [SerializeField] Sprite VibrationOn;
            [Tooltip("������ ����������� ��������")]
            [SerializeField] Sprite VibrationOff;
            private bool hasVibration;
            public bool IsVibration
            {
                get { return hasVibration; }
                set
                {
                    hasVibration = value;
                    ChangeVibration(hasVibration);
                }
            }
            public void StartVibration()
            {

                float temp = PlayerPrefs.GetFloat("Vibration", 1);
                if (temp == 0)
                {
                    hasVibration = false;
                    VibrationIconRenderer.sprite = VibrationOff;
                }
                else
                {
                    hasVibration = true;
                    VibrationIconRenderer.sprite = VibrationOn;
                }
            }
            private void ChangeVibration(bool value)
            {
                if (value)
                {
                    VibrationIconRenderer.sprite = VibrationOn;
                    PlayerPrefs.SetFloat("Vibration", 1);
                }
                else
                {
                    VibrationIconRenderer.sprite = VibrationOff;
                    PlayerPrefs.SetFloat("Vibration", 0);
                }
            }

            public void Vibrate(HapticTypes Haptictype)
            {
                if (Singleton.settings.Vibration.IsVibration)
                {
                    MMVibrationManager.Haptic(Haptictype);
                }
            }
        };
    }
    [Serializable]
    public class Money
    {
        private int money;
        [Tooltip("UI ����� ��� ����������� �����")]
        [SerializeField] Text moneyText;
        public int GetMoney()
        {
            return money;
        }
        public void StartMoney()
        {
            money = PlayerPrefs.GetInt("Money");
            moneyText.text = money.ToString();
        }
        public void AddMoney(int value)
        {
            money += value;
            moneyText.text = money.ToString();
            PlayerPrefs.SetInt("Money", money);
        }
    }
    
    [Serializable]
    public class LevelManager {
        [Tooltip("���� ����� ����� ������ ����������� ��������, ���� ��� �� ����� ��������")]
        [SerializeField] bool isRandom;
        [Tooltip("���� ��������� ������(GameObject)")]
        [SerializeField] List<GameObject> Levels = new List<GameObject>();
        private GameObject curLevelObj;
        private int curLevel;
        public void StartLevel() {
            curLevel = PlayerPrefs.GetInt("Level");
            curLevelObj = Instantiate(Levels[curLevel]);
        }
        public void NextLevel() {
            Destroy(curLevelObj);
            if (isRandom) {
                int temp = curLevel;
                while(temp==curLevel)
                temp = UnityEngine.Random.Range(0,Levels.Count);
                curLevel = temp;
            }
            else
            {
                curLevel++;
                if (curLevel == Levels.Count) curLevel = 0;
                PlayerPrefs.SetInt("Level", curLevel);
            }
            curLevelObj = Instantiate(Levels[curLevel]);
        }
        public void RestartLevel() {
            Destroy(curLevelObj);
            curLevelObj = Instantiate(Levels[curLevel]);
        }
    }
    [System.Serializable]
    public class Sounds
    {
        //����� ��������� ����
        [SerializeField] AudioClip sampleClip;
        public void PlaySound(string clipName)
        {
            if (Singleton.settings.Audio.IsAudio)
            {
                switch (clipName)
                {
                    //���� ��������� ����� ��� �������, ��� ����� ������
                    case "zap": Singleton.settings.Audio.PlaySound(sampleClip); break;
                }
            }
        }
    }
    #endregion
}