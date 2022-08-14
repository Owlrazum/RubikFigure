using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;

using static Orazum.Collections.CollectionUtilities;

namespace Orazum.Collections
{ 
    /// <summary>
    /// Needs testing, should not rely on it yet.
    /// </summary>
    public static class Algorithms
    {
        public const int MAX_ITERATION_COUNT = 10000;

        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            for (int i = n - 1; i >= 1; i--)
            {
                int j = Random.Range(0, i + 1);

                T value = list[j];
                list[j] = list[i];
                list[i] = value;
            }
        }

        // https://epubs.siam.org/doi/pdf/10.1137/1.9781611972986.7 
        public static T[] RandomDerangement<T>(T[] list)
        {
            List<int> marks = new List<int>(list.Length - 1);
            List<int> possible = new List<int>(list.Length - 1);

            T[] derangement = new T[list.Length];

            for (int i = 0; i < list.Length; i++)
            {
                int d = Random.Range(0, list.Length - 1 - marks.Count);
                Debug.Log(d + "sdsd");
                for (int j = 0; j < list.Length; j++)
                {
                    if (!Contains(marks, j) && j != i)
                    { 
                        possible.Add(j);
                    }
                }
                Debug.Log(LogUtilities.ToLog(marks) + "\n" + LogUtilities.ToLog(possible));
                derangement[possible[d]] = list[i];
                marks.Add(possible[d]);
                possible.Clear();
            }

            Debug.Log(LogUtilities.ToLog(derangement));
            return derangement;
        }

        private static bool Contains(List<int> list, int value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i] == value)
                {
                    return true;
                }
            }
            return false;
        }

        private static bool CheckIterationCount(ref int iterationCount)
        {
            iterationCount++;
            if (iterationCount > MAX_ITERATION_COUNT)
            {
                Debug.LogError("IterationCountExceeded");
                return false;
            }
            return true;
        }
    }
}
/*
public static void RandomDerangement<T>(IList<T> list)
        {
            ValidIndicesByBool marks = new ValidIndicesByBool(list.Count);
            int i = list.Count - 1;
            int u = list.Count - 1;

            int iterationCount = 0;
            while (u >= 1)
            {
                if (!marks.GetIndex(i))
                {
                    int j;
                    do
                    {
                        j = Random.Range(0, i - 1);
                        if (!CheckIterationCount(ref iterationCount)) return;
                    } while (marks[j]);
                    list.Swap(i, j);
                    if (Random.value < (u - 1) * SubFactorial(u - 2) / SubFactorial(u))//u / list.Count) // is a hack, should use 
                    {
                        marks.Get[j] = true;
                        u--;
                    }
                    u--;
                }
                i--;
                if (!CheckIterationCount(ref iterationCount)) return;
            }
        }
*/