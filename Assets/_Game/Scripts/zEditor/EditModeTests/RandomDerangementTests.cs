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
        for (int i = 0; i < count; i++)
        {
            original[i] = i;
        }

        for (int i = 0; i < 5; i++)
        { 
            int[] deranged = Algorithms.RandomDerangement(in original);
            ValidateDerangement(original, deranged);
            ResetDerangement(deranged);
        }
    }

    private void ValidateDerangement(int[] original, int[] deranged)
    {
        for (int i = 0; i < deranged.Length; i++)
        { 
            Assert.AreNotEqual(deranged[i], original[i]);
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