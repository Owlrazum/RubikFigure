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
    private const int vibrationTime = 50;

    [Header("Custom Vibrations")]
    [Space]
    [SerializeField]
    private string differentiateFields;

    [Serializable]
    public struct VibrationData
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

    public void VibrateOnChangeHapticSetting()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }

        Vibration.Vibrate(vibrationTime);
    }


    private void Awake()
    {
        Vibration.Init();

        InitializeFirstVibrationPattern();
        InitializeSecondVibrationPattern();



        // Events subscriptions below:
    }

    private void Vibrate(int vibrationTimeArg = vibrationTime)
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }
        Vibration.Vibrate(vibrationTime);
    }

    private void StopVibration()
    {
        if (isVibrating)
        { 
            Vibration.Cancel();
            OnNextFrameFalseIsVibrating();
        }
    }

    private IEnumerator OnNextFrameFalseIsVibrating()
    {
        yield return null;
        isVibrating = false;
    }

    private VibrationPattern firstVibrationPattern;
    private VibrationPattern secondVibrationPattern;

    private void InitializeFirstVibrationPattern()
    {
        firstVibrationPattern = new VibrationPattern
            (firstStartDelay, firstVibrationPatternData, shouldRepeatFirstPattern);
    }

    private void InitializeSecondVibrationPattern()
    {
        secondVibrationPattern = new VibrationPattern
            (secondStartDelay, secondVibrationPatternData, shouldRepeatSecondPattern);
    }

    private bool isVibrating;

    private void OnFirstPattern()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }

        if (!isVibrating)
        {
            isVibrating = true;
            firstVibrationPattern.StartPlaying();
        }
    }

    private void OnSecondPattern()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }

        if (!isVibrating)
        {
            isVibrating = true;
            secondVibrationPattern.StartPlaying();
        }
    }
}
