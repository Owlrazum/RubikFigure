using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sample : MonoBehaviour
{
	public void PlayVibrate() {

        SettingsManager.Singleton.PlaySound("sampleClip");
        SettingsManager.Singleton.Vibrate('l');
    }
}