using UnityEngine;

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

    private void Awake()
    {
    }

    private void OnDestroy()
    {
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
}