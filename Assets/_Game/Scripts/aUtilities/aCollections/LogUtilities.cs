using System.Collections.Generic;

public static class LogUtilities
{
    public static string ToLog<TKey, TValue>(Dictionary<TKey, TValue> toLog)
    {
        string log = "";
        foreach (var entry in toLog)
        {
            log += entry.Key + " " + entry.Value + "\n";
        }

        return log;
    }
}