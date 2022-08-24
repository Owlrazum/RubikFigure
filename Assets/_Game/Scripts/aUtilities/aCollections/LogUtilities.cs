using System.Text;
using System.Collections.Generic;
using Unity.Collections;

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

        public enum DecimalPlacesAfterDot
        {
            Unspecified,
            One,
            Two,
            Four
        }
        public static string ToLog<T>(NativeArray<T> array, 
            DecimalPlacesAfterDot places = DecimalPlacesAfterDot.Unspecified, 
            int elementsBeforeNewLine = 5) where T : struct
        {
            string log = "";
            int counter = 0;
            for (int i = 0; i < array.Length; i++)
            {
                switch (places)
                { 
                    case DecimalPlacesAfterDot.Unspecified:
                        log += $"{array[i]} ";
                        break;
                    case DecimalPlacesAfterDot.One:
                        log += $"{array[i]:F1} ";
                        break;
                    case DecimalPlacesAfterDot.Two:
                        log += $"{array[i]:F2} ";
                        break;
                    case DecimalPlacesAfterDot.Four:
                        log += $"{array[i]:F4} ";
                        break;
                }
                counter++;
                if (counter >= elementsBeforeNewLine)
                {
                    log += "\n";
                    counter = 0;
                }
            }
            return log;
        }
    }
}