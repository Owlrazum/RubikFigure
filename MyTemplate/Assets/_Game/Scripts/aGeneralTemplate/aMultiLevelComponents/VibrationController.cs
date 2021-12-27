using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// It was copied from the chainsaw. Needs modifying so it will fit general template.
/// </summary>
public class VibrationController : MonoBehaviour
{
    [Header("Default Vibration")]
    [Space]
    [SerializeField]
    [Tooltip("In milliseconds")]
    private int vibrationTime = 200;

    [Header("Custom Vibration")]
    [Space]
    [SerializeField]
    private string differentiateFields;

    [Serializable]
    private struct VibrationData
    {
        public int vibrationTime;
        public int pauseTime;
    }

    [Header("Saw Vibration")]
    [Space]
    [SerializeField]
    [Tooltip("If not needed set it to zero.\n" +
    "Unity developer has no power to remove it...")]
    private int sawStartDelay = 0;

    [SerializeField]
    [Tooltip("Use it if there is a custom vibration with a pattern.\n" +
        "Vibr, pause, vibr, pause...")]
    private List<VibrationData> sawVibrationPatternData;

    [SerializeField]
    private bool shouldRepeatSawPattern = true;

    [Header("Fail Vibration")]
    [Space]
    [SerializeField]
    [Tooltip("If not needed set it to zero.\n" +
    "Unity developer has no power to remove it...")]
    private int failStartDelay = 0;

    [SerializeField]
    [Tooltip("Use it if there is a custom vibration with a pattern.\n" +
        "Vibr, pause, vibr, pause...")]
    private List<VibrationData> failVibrationPatternData;

    [SerializeField]
    private bool shouldRepeatFailPattern = true;

    [Header("Grind Vibration")]
    [Space]
    [SerializeField]
    [Tooltip("If not needed set it to zero.\n" +
    "Unity developer has no power to remove it...")]
    private int grindStartDelay = 0;

    [SerializeField]
    [Tooltip("Use it if there is a custom vibration with a pattern.\n" +
        "Vibr, pause, vibr, pause...")]
    private List<VibrationData> grindVibrationPatternData;

    [SerializeField]
    private bool shouldRepeatGrindPattern = true;

    [Header("Color Vibration")]
    [Space]
    [SerializeField]
    [Tooltip("If not needed set it to zero.\n" +
    "Unity developer has no power to remove it...")]
    private int colorStartDelay = 0;

    [SerializeField]
    [Tooltip("Use it if there is a custom vibration with a pattern.\n" +
        "Vibr, pause, vibr, pause...")]
    private List<VibrationData> colorVibrationPatternData;

    [SerializeField]
    private bool shouldRepeatColorPattern = true;

    [Header("Stameska")]
    [Space]
    [SerializeField]
    private long stameskaVibrationTime;


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
                StopCuttingVibration();
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

    private long[] sawVibrationPattern;
    private long[] grindVibrationPattern;
    private long[] colorVibrationPattern;

    private long[] failVibrationPattern;

    private void Awake()
    {
        Vibration.Init();

        InitializeSawVibrationPattern();
        InitializeFailVibrationPattern();
        InitializeGrindVibrationPattern();
        InitializeColorVibrationPattern();
    }

    private void InitializeSawVibrationPattern()
    {
        sawVibrationPattern = new long[1 + sawVibrationPatternData.Count * 2];
        sawVibrationPattern[0] = sawStartDelay;

        int index = 1;
        for (int i = 0; i < sawVibrationPatternData.Count; i++)
        {
            sawVibrationPattern[index++] = sawVibrationPatternData[i].vibrationTime;
            sawVibrationPattern[index++] = sawVibrationPatternData[i].pauseTime;
        }
    }

    private void InitializeGrindVibrationPattern()
    {
        grindVibrationPattern = new long[1 + grindVibrationPatternData.Count * 2];
        grindVibrationPattern[0] = grindStartDelay;

        int index = 1;
        for (int i = 0; i < grindVibrationPatternData.Count; i++)
        {
            grindVibrationPattern[index++] = grindVibrationPatternData[i].vibrationTime;
            grindVibrationPattern[index++] = grindVibrationPatternData[i].pauseTime;
        }
    }

    private void InitializeColorVibrationPattern()
    {
        colorVibrationPattern = new long[1 + colorVibrationPatternData.Count * 2];
        colorVibrationPattern[0] = colorStartDelay;

        int index = 1;
        for (int i = 0; i < colorVibrationPatternData.Count; i++)
        {
            colorVibrationPattern[index++] = colorVibrationPatternData[i].vibrationTime;
            colorVibrationPattern[index++] = colorVibrationPatternData[i].pauseTime;
        }
    }

    private void InitializeFailVibrationPattern()
    {
        failVibrationPattern = new long[1 + failVibrationPatternData.Count * 2];
        failVibrationPattern[0] = failStartDelay;

        int index = 1;
        for (int i = 0; i < failVibrationPatternData.Count; i++)
        {
            failVibrationPattern[index++] = failVibrationPatternData[i].vibrationTime;
            failVibrationPattern[index++] = failVibrationPatternData[i].pauseTime;
        }
    }

    private bool isVibrating;

    public void StartCuttingVibration()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }

        if (!isVibrating)
        {
            isVibrating = true;
            Vibration.Vibrate(sawVibrationPattern, shouldRepeatSawPattern ? 0 : -1);
        }
    }

    public void StopCuttingVibration()
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

    public void VibrateForStameska()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }
        Vibration.Vibrate(stameskaVibrationTime);
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
            Vibration.Vibrate(failVibrationPattern, shouldRepeatFailPattern ? 0 : -1);
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

    public void StartGrindingVibration()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }

        if (!isVibrating)
        {
            isVibrating = true;
            Vibration.Vibrate(grindVibrationPattern, shouldRepeatGrindPattern ? 0 : -1);
        }
    }

    public void StopGrindingVibration()
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

    public void StartColorVibration()
    {
        if (!IsAllowedToVibrate)
        {
            return;
        }

        if (!isVibrating)
        {
            isVibrating = true;
            Vibration.Vibrate(colorVibrationPattern, shouldRepeatColorPattern ? 0 : -1);
        }
    }

    public void StopColorVibration()
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
