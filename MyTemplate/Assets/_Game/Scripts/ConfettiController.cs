using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class ConfettiController : MonoBehaviour
{
    private List<ParticleSystem> confettiParticles;

    private void Awake()
    {
        confettiParticles = new List<ParticleSystem>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var confetti = transform.GetChild(i).GetComponent<ParticleSystem>();
            if (confetti != null)
            {
                confettiParticles.Add(confetti);
            }
        }
    }

    /// <summary>
    /// Add it to the GameManager, if you are Abai.
    /// </summary>
    public void ThrowSomeConfetti()
    {
        foreach (ParticleSystem c in confettiParticles)
        {
            c.Play();
        }
    }
}
