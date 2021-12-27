using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    /// <summary>
    /// There should be multiple AudioSources as children of the this transform.
    /// If needed, for each AudioSource there can be mutliple AudioClips.
    ///
    /// Start and stop looping sound worked good in chainsaw.
    /// </summary>
    public class SoundController : MonoBehaviour
    {
        /*
         * Useful Methods for AudioSource:
         * Play(), Stop(), PlayOneShot(), PlayDelayed()
         * 
         */

        [SerializeField]
        private AudioSource source1;

        [SerializeField]
        private AudioClip clipForS1_1;

        [SerializeField]
        private AudioClip clipForS1_2; // ...

        [SerializeField]
        private AudioSource turnOnSound;

        [SerializeField]
        private AudioSource winSound;

        private void Awake()
        {
            if (source1 != null)
            {
                source1.clip = clipForS1_1;
            }
        }

        private bool shouldProduceSound;
        public bool ShouldProduceSound
        {
            get
            {
                return shouldProduceSound;
            }
            set
            {
                shouldProduceSound = value;
                if (shouldProduceSound)
                {
                    AudioListener.volume = 1;
                }
                else
                {
                    AudioListener.volume = 0;
                }
            }
        }

        public void StartPlayingSoundFromSource1()
        {
            source1.Play();
        }

        public void PlaySecondClipFromSource1()
        {
            source1.Stop();
            source1.PlayOneShot(clipForS1_2);
            source1.PlayDelayed(clipForS1_2.length);
        }

        public void StopSoundFromSource1()
        {
            source1.Stop();
        }

        public void PlaySoundTurnOn()
        {
            turnOnSound.Play();
            print("SoundTurnOn");
        }

        public void PlayWinSound()
        {
            winSound.Play();
            print("WinSOund");
        }
    }
}
