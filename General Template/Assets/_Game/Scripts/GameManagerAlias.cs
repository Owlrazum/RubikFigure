using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GeneralTemplate;

public static class General
{
    public static void AlternateHaptic()
    {
        GameManager.Singleton.AlternateHaptic();
    }

    public static void AlternateSound()
    {
        GameManager.Singleton.AlternateSound();
    }

    public static void EndLevel(GameResult result)
    {
        GameManager.Singleton.EndGame(result);
    }

    public static void PlaySound(AudioClip clip)
    {
        GameManager.Singleton.PlaySound(clip);
    }
}
