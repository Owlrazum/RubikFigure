using System.Collections.Generic;

public class VibrationPattern
{
    private long[] vibrationPattern;

    /// <summary>
    /// In seconds.
    /// </summary>
    private float timeForOnePlay;

    private const float MILLISECOND_TO_SECOND = 1 / 1000.0f;

    public VibrationPattern(
        int startDelayArg,
        List<VibrationController.VibrationData> patternArg
    )
    {
        vibrationPattern = new long[1 + patternArg.Count * 2];
        vibrationPattern[0] = startDelayArg;

        timeForOnePlay = MILLISECOND_TO_SECOND * startDelayArg;

        int index = 1;
        for (int i = 0; i < patternArg.Count; i++)
        {
            vibrationPattern[index++] = patternArg[i].vibrationTime;
            vibrationPattern[index++] = patternArg[i].pauseTime;

            timeForOnePlay += MILLISECOND_TO_SECOND * (patternArg[i].vibrationTime + patternArg[i].pauseTime);
        }
    }

    public void PlayOnce()
    { 
        Vibration.Vibrate(vibrationPattern, -1); // will play once for -1, play loop for 0.
    }

    public float GetTimeForOnePlay()
    {
        return timeForOnePlay;
    }
}