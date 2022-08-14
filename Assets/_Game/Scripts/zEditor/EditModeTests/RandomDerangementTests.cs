using NUnit.Framework;
using System.Collections.Generic;

using Orazum.Collections;

public class RandomDerangementTests
{
    [Test]
    public void Tests()
    {
        int count = 100;
        int[] original = new int[count];
        int[] deranged = new int[count];
        for (int i = 0; i < count; i++)
        {
            original[i] = i;
            deranged[i] = i;
        }

        for (int i = 0; i < 5; i++)
        { 
            Algorithms.RandomDerangement(deranged);
            ValidateDerangement(original, deranged);
            ResetDerangement(deranged);
        }
    }

    private void ValidateDerangement(int[] original, int[] deranged)
    {
        for (int i = 0; i < deranged.Length; i++)
        { 
            Assert.AreNotEqual(original[i], deranged[i]);
        }
    }

    private void ResetDerangement(int[] deranged)
    {
        for (int i = 0; i < deranged.Length; i++)
        {
            deranged[i] = i;
        }
    }
}