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

    private void Awake()
    {
        Vibration.Init();

        InitializeFirstVibrationPattern();
        InitializeSecondVibrationPattern();

        // Events subscriptions below:

    }

    private void OnDestroy()
    {
        // Events unsubscriptions below:

    }

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
                StopVibration();
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

    private void Vibrate()
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
            StartCoroutine(OnNextFrameFalseIsVibrating());
        }
    }

    private IEnumerator OnNextFrameFalseIsVibrating()
    {
        yield return null;
        isVibrating = false;
        shouldRepeatAnyPattern = false;
    }

    private VibrationPattern firstVibrationPattern;
    private VibrationPattern secondVibrationPattern;

    private void InitializeFirstVibrationPattern()
    {
        firstVibrationPattern = new VibrationPattern
            (firstStartDelay, firstVibrationPatternData);
    }

    private void InitializeSecondVibrationPattern()
    {
        secondVibrationPattern = new VibrationPattern
            (secondStartDelay, secondVibrationPatternData);
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
            if (shouldRepeatFirstPattern)
            {
                StartCoroutine(RepeatingPattern(firstVibrationPattern));
            }
            else
            { 
                firstVibrationPattern.PlayOnce();
            }
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
            if (shouldRepeatSecondPattern)
            {
                StartCoroutine(RepeatingPattern(secondVibrationPattern));
            }
            else
            { 
                secondVibrationPattern.PlayOnce();
            }
        }
    }

    private bool shouldRepeatAnyPattern;
    private IEnumerator RepeatingPattern(VibrationPattern pattern)
    {
        shouldRepeatAnyPattern = true;
        while (shouldRepeatAnyPattern)
        {
            pattern.PlayOnce();
            yield return new WaitForSeconds(pattern.GetTimeForOnePlay());
        }
    }
}
