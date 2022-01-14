using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Subscribe to necessary events with relevant methods.
/// </summary>
public class VibrationController : MonoBehaviour
{
    [Header("Default Vibration")]
    [Space]
    [SerializeField]
    [Tooltip("In milliseconds")]
    private int vibrationTime = 50;

    [Header("Custom Vibrations")]
    [Space]
    [SerializeField]
    private string differentiateFields;

    [Serializable]
    private struct VibrationData
    {
        public int vibrationTime;
        public int pauseTime;
    }

    [Header("First type Vibration sequence")]
    [Space]
    [SerializeField]
    [Tooltip("If not needed set it to zero.\n" +
    "Unity developer has no power to remove it...")]
    private int firstStartDelay = 0;

    [SerializeField]
    [Tooltip("Use it if there is a custom vibration with a pattern.\n" +
        "Vibr, pause, vibr, pause...")]
    private List<VibrationData> firstVibrationPatternData;

    [SerializeField]
    private bool shouldRepeatFirstPattern = true;

    [Header("Second type Vibration sequence")]
    [Space]
    [SerializeField]
    [Tooltip("If not needed set it to zero.\n" +
    "Unity developer has no power to remove it...")]
    private int secondStartDelay = 0;

    [SerializeField]
    [Tooltip("Use it if there is a custom vibration with a pattern.\n" +
        "Vibr, pause, vibr, pause...")]
    private List<VibrationData> secondVibrationPatternData;

    [SerializeField]
    private bool shouldRepeatSecondPattern = true;

    private bool isAllowedToVibrate;
    public bool IsAllowedToVibrate
    {
        get
        {
            return isAllowedToVibrate;
        }
        set
        {
            isAllowedToVibrate = value;
            if (!isAllowedToVibrate)
            {
                //StopCuttingVibration();
            }
        }
    }

    public void Vibrate()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }
        Vibration.Vibrate(vibrationTime);
    }

    private long[] firstVibrationPattern;
    private long[] secondVibrationPattern;

    private void Awake()
    {
        Vibration.Init();

        InitializeFirstVibrationPattern();
        InitializeSecondVibrationPattern();
    }

    private void InitializeFirstVibrationPattern()
    {
        firstVibrationPattern = new long[1 + firstVibrationPatternData.Count * 2];
        firstVibrationPattern[0] = firstStartDelay;

        int index = 1;
        for (int i = 0; i < firstVibrationPatternData.Count; i++)
        {
            firstVibrationPattern[index++] = firstVibrationPatternData[i].vibrationTime;
            firstVibrationPattern[index++] = firstVibrationPatternData[i].pauseTime;
        }
    }

    private void InitializeSecondVibrationPattern()
    {
        secondVibrationPattern = new long[1 + secondVibrationPatternData.Count * 2];
        secondVibrationPattern[0] = secondStartDelay;

        int index = 1;
        for (int i = 0; i < secondVibrationPatternData.Count; i++)
        {
            secondVibrationPattern[index++] = secondVibrationPatternData[i].vibrationTime;
            secondVibrationPattern[index++] = secondVibrationPatternData[i].pauseTime;
        }
    }

    private bool isVibrating;

    public void OnFirstVibration()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }

        if (!isVibrating)
        {
            isVibrating = true;
            Vibration.Vibrate(firstVibrationPattern, shouldRepeatFirstPattern ? 0 : -1);
        }
    }

    public void OnCuttingVibration()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }

        if (isVibrating)
        {
            StartCoroutine(OnNextFrameFalseIsVibrating());
            Vibration.Cancel();
        }
    }


    private IEnumerator OnNextFrameFalseIsVibrating()
    {
        yield return null;
        isVibrating = false;
    }

    public void StartFailVibration()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }

        if (!isVibrating)
        {
            isVibrating = true;
            Vibration.Vibrate(secondVibrationPattern, shouldRepeatSecondPattern ? 0 : -1);
        }
    }

    public void StopFailVibration()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }

        if (isVibrating)
        {
            StartCoroutine(OnNextFrameFalseIsVibrating());
            Vibration.Cancel();
        }
    }
}
