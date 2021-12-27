using UnityEngine;

using GeneralTemplate;

// This class is for you if you do not want to write GameManager.Singleton;
// To be honest, I do not see it as a problem, but it is for some developer.
// I will not point a finger at him.
public static class General
{
    public static void StartGame()
    {
        GameManager.Singleton.StartGame();
    }

    public static void EndCurrentLevel()
    {
        GameManager.Singleton.EndCurrentLevel();
    }

    public static void PlaySound(AudioClip clip)
    {
        GameManager.Singleton.PlaySound();
    }

    public static void PlayParticles(Vector3 position)
    {
        GameManager.Singleton.PlayParticles(position);
    }
}
