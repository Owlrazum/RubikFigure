using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Sample : MonoBehaviour
{
    public void AddMoney()
    {
        General.AddMoney(1);
    }
    public void PlaySoundAndVibrate()
    {
        General.PlaySound("zap");
        General.Vibrate(MoreMountains.NiceVibrations.HapticTypes.Warning);
    }
    public void WinGame() {
        General.WinGame();
    }
    public void LoseGame() {
        General.LoseGame();
    }
    public void NextLvl() {
        General.NextLevel();
    }
    public void RestartLvl() {
        General.RestartLevel();
    }
}