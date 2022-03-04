using UnityEngine;

namespace GeneralTemplate
{
    /// <summary>
    /// There should be multiple AudioSources as children of this transform.
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
        private AudioSource turnOnSound;

        [SerializeField]
        private AudioSource winSound;

        private void Awake()
        {
            GeneralEventsContainer.LevelComplete += OnLevelComplete;
        }

        private void OnDestroy()
        { 
            GeneralEventsContainer.LevelComplete -= OnLevelComplete;
        }

        private void OnLevelComplete(int notUsed)
        {
            winSound.Play();
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

        public void PlaySoundTurnOn()
        {
            turnOnSound.Play();
        }
    }
}
