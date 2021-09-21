using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.NiceVibrations;
public static class General
{
    public static void PlaySound(string value)
    {
        SettingsManager.Singleton.audioSounds.PlaySound(value);//Audio Clip
    }
    public static void Vibrate(HapticTypes type)
    {
        SettingsManager.Singleton.settings.Vibration.Vibrate(type); //Haptic Type
    }
    public static void WinGame()
    {
        SettingsManager.Singleton.WinLvl();
    }
    public static void LoseGame()
    {
        SettingsManager.Singleton.LoseLvl();
    }
    public static void NextLevel()
    {
        SettingsManager.Singleton.NextLevel();
    }
    public static void RestartLevel()
    {
        SettingsManager.Singleton.RestartLevel();
    }
    public static void AddMoney(int value)
    {
        SettingsManager.Singleton.AddMoney(value);
    }
    public static int GetMoney()
    {
        return SettingsManager.Singleton.GetMoney();
    }
}