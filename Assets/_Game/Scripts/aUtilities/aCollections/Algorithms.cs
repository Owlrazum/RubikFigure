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

        public static T[] RandomDerangement<T>(T[] list)
        {
            List<int> marks = new List<int>(list.Length - 1);
            List<int> possible = new List<int>(list.Length - 1);

            T[] derangement = new T[list.Length];
            bool needsShuffleLastElement = false;

            for (int i = 0; i < list.Length; i++)
            {
                int d = Random.Range(0, list.Length - marks.Count - 1);
                for (int j = 0; j < list.Length; j++)
                {
                    if (!Contains(marks, j) && j != i)
                    { 
                        possible.Add(j);
                    }
                }
                if (possible.Count == 0)
                {
                    needsShuffleLastElement = true;
                    break;
                }
                derangement[possible[d]] = list[i];
                marks.Add(possible[d]);
                possible.Clear();
            }

            if (needsShuffleLastElement)
            {
                int rnd = Random.Range(0, list.Length - 1);
                T value = derangement[rnd];
                derangement[rnd] = list[list.Length - 1];
                derangement[list.Length - 1] = value;
            }

            return derangement;
        }
    }
}
