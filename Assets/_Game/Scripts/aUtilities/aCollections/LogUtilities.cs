using System.Collections.Generic;

namespace Orazum.Collections
{ 
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

        public static string ToLog<T>(IList<T> list)
        {
            string log = "";
            for (int i = 0; i < list.Count; i++)
            {
                log += list[i] + " ";
            }
            return log;
        }
    }
}