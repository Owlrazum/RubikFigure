using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using MoreMountains.NiceVibrations;
public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Singleton;
    public Settings settings;
    [SerializeField] GameObject PausePanel;
    [SerializeField] GameObject WinPanel;
    [SerializeField] GameObject LosePanel;
    [Space]
    private float timeScale;
    public Sounds audioSounds;
    void Awake()
    {
        Singleton = this;
        PlayerPrefs.GetInt("Lvl");
    }
    private void Start()
    {
        settings.Audio.AudioManager = GameObject.Find("[AudioManager]").GetComponent<AudioSource>();
        settings.Audio.CheckSound();
        settings.Vibration.CheckVibration();
    }
    public void NextLevel(int nextLvlIndx)
    {
        SceneManager.LoadScene(nextLvlIndx);
    }
    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void WinGame()
    {
        WinPanel.SetActive(true);
    }
    public void LoseGame()
    {
        LosePanel.SetActive(true);
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
    public void PlaySound(string value) {
        audioSounds.PlaySound(value);
    }
    public void Vibrate(char type) {
        settings.Vibration.Vibrate(type);
    }
    public void SoundSet()
    {
        settings.Audio.IsAudio = !settings.Audio.IsAudio;
    }
    public void VibrationSet()
    {
        settings.Vibration.IsVibration = !settings.Vibration.IsVibration;
    }
    [System.Serializable]
    public class Settings
    {
        public AudioSettings Audio;
        public VibrationSettings Vibration;
        [System.Serializable]
        public class AudioSettings
        {
            [SerializeField] Image SoundIconRenderer;
            [SerializeField] Sprite SoundOn;
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
            public void CheckSound()
            {
                float temp = PlayerPrefs.GetFloat("AudioSet", 1);
                if (temp == 0)
                {
                    hasAudio = false;
                    AudioManager.volume = 0;
                    SoundIconRenderer.sprite = SoundOff;
                }
                else
                {
                    hasAudio = true;
                    AudioManager.volume = 1;
                    SoundIconRenderer.sprite = SoundOn;
                }
            }
            public void ChangeSounnd(bool value)
            {
                if (value)
                {
                    SoundIconRenderer.sprite = SoundOn;
                    AudioManager.volume = 1;
                    PlayerPrefs.SetFloat("AudioSet", AudioManager.volume);
                }
                else
                {
                    SoundIconRenderer.sprite = SoundOff;
                    AudioManager.volume = 0;
                    PlayerPrefs.SetFloat("AudioSet", AudioManager.volume);
                }
            }
        }
        [System.Serializable]
        public class VibrationSettings
        {
            [SerializeField] Image VibrationIconRenderer;
            [SerializeField] Sprite VibrationOn;
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
            public void CheckVibration()
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

            public void Vibrate(char type)
            {
                if (Singleton.settings.Vibration.IsVibration)
                {
                    switch (type)
                    {
                        case 'l': MMVibrationManager.Haptic(HapticTypes.Warning); break;
                        case 'h': MMVibrationManager.Haptic(HapticTypes.Failure); break;
                    }
                }
            }
        };
    }
    [System.Serializable]
    public class Sounds
    {
        [SerializeField] AudioClip sampleClip;
        public void PlaySound(string clipName)
        {
            if (Singleton.settings.Audio.IsAudio)
            {
                switch (clipName)
                {
                    case "sampleClip": Singleton.settings.Audio.PlaySound(sampleClip); break;
                }
            }
        }
    }
}