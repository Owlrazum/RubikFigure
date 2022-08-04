using System.Collections.Generic;
using UnityEngine;

public class ParticlesController : MonoBehaviour
{
    private Queue<ParticleSystem> particles;

    private void Start()
    {
        particles = new Queue<ParticleSystem>();
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var particle = transform.GetChild(i).GetComponent<ParticleSystem>();
            particles.Enqueue(particle);
        }
    }

    public void PlayParticles(Vector3 position)
    {
        var p = particles.Dequeue();
        if (p.isPlaying)
        {
            p.Stop();
        }
        p.transform.position = position;
        p.Play();
        particles.Enqueue(p);
    }
}
