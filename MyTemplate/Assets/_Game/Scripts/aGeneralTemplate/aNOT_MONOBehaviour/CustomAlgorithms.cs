using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Needs testing, should not rely on it yet.
/// </summary>
public static class CustomAlgorithms
{
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
}
