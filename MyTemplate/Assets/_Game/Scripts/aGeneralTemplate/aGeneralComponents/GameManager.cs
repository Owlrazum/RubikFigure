using UnityEngine;

namespace GeneralTemplate
{
    // TODO Decide whether ParticlesController should use ObjectPool;
    // TODO Custom Mechanics, remove from project and make packages. Or do not.

    // TODO Update scenes L1 L2, add prefabs (eg LevelDataHandover)

    // TODO Make a virtual class for fading animations.

    // Tip: There is a confetti plane in the main camera.

    // TODO make a patterns text somewhere, DoRotateSequence after each stage.

    // TODO Implement an EventContainer instead of layered architecture, because events are more fitted to our case.

    /// <summary>
    /// General functionality in our projects. 
    /// It is likely that no modifications will be needed.
    /// It is also desirable for it to be the only Singleton Monobehaviour.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Singleton;
        private void Awake()
        {
            if (Singleton == null)
            {
                Singleton = this;
            }
            else
            {
                Destroy(multiLevelsParent);
            }

            if (!shouldDestroyOnSceneLoad)
            {
                DontDestroyOnLoad(multiLevelsParent);
            }
        }

        [Header("MultiLevels")]
        [Space]
        [SerializeField]
        private bool shouldDestroyOnSceneLoad;
        
        [SerializeField]
        private GameObject multiLevelsParent;

        [SerializeField]
        private UIGeneralController userInterface;

        [SerializeField]
        private SoundController soundController;

        [SerializeField]
        private VibrationController vibrationController;

        private void Start()
        {
            InitializeSettings();
        }

        private void InitializeSettings()
        {
            hasSound = PlayerPrefs.GetInt("Sound") == 0;
            isHaptic = PlayerPrefs.GetInt("Haptic") == 0;

            soundController.ShouldProduceSound = hasSound;
            userInterface.SetSoundPresentation(hasSound);

            vibrationController.IsAllowedToVibrate = isHaptic;
            userInterface.SetVibrationPresentation(isHaptic);
        }

        #region SettingsAlternations

        private bool isHaptic;
        public void AlternateHaptic()
        {
            isHaptic = !isHaptic;
            PlayerPrefs.SetInt("Haptic", isHaptic ? 0 : 1);
            vibrationController.IsAllowedToVibrate = isHaptic;
            vibrationController.VibrateOnChangeHapticSetting();
        }

        private bool hasSound;
        public void AlternateSound()
        {
            hasSound = !hasSound;
            PlayerPrefs.SetInt("Sound", hasSound ? 0 : 1);
            soundController.ShouldProduceSound = hasSound;
            if (hasSound)
            {
                soundController.PlaySoundTurnOn();
            }
        }

        #endregion
    }
}


