using System.Collections.Generic;

public class VibrationPattern
{
    private long[] vibrationPattern;
    private bool shouldRepeatPattern;
    public VibrationPattern(
            int startDelayArg,
            List<VibrationController.VibrationData> patternArg,
            bool shouldRepeatPatternArg)
    {
        shouldRepeatPattern = shouldRepeatPatternArg;

        vibrationPattern = new long[1 + patternArg.Count * 2];
        vibrationPattern[0] = startDelayArg;

        int index = 1;
        for (int i = 0; i < patternArg.Count; i++)
        {
            vibrationPattern[index++] = patternArg[i].vibrationTime;
            vibrationPattern[index++] = patternArg[i].pauseTime;
        }
    }

    public void StartPlaying()
    { 
        Vibration.Vibrate(vibrationPattern, shouldRepeatPattern ? 0 : -1);
    }
}