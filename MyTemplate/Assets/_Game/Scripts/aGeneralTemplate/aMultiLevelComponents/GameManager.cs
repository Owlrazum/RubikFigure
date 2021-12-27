using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    // TODO Decide whether ParticlesController should use ObjectPool;
    // TODO Custom Mechanics, remove from project and make packages. Or do not.

    // TODO Update scenes L1 L2, add prefabs (eg LevelDataHandover)

    // TODO Make a virtual class for fading animations.

    // TODO Generalize vibrationController.

    // Perhaps there is more to be taken from the saw.

    // Tip: There is a confetti plane in the main camera.


    // TODO Take from GameManager of chainsaw

    // TODO Export package with the clients and ordering phase.

    // TODO make a patterns text somewhere, DoRotateSequence after each stage.


    // BIG General TODO: Separate gameplay logic from GameManager. Make Hiearchy of Singletons,
    // where GameManager is accesible only from gameplay managers. Each Manager should have its
    // Singleton. Gameplay dependent systems should be accesisble from gameplay managers,
    // but not from GameManager.


    public class GameManager : MonoBehaviour
    {

        // Generally, you should not modify General Code section.

        // ========= General Code =========
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
        }

        [SerializeField]
        private LevelType levelType;
        
        [SerializeField]
        private GameObject multiLevelsParent;

        private void Start()
        {
            if (levelType == LevelType.Scene)
            {
                DontDestroyOnLoad(multiLevelsParent);
                scenesController.LoadSavedScene();
            }
            else if (levelType == LevelType.Prefab)
            {
                prefabLevelsController.LoadSavedLevel();
            }
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

        [Header("GeneralTemplateComponents")]
        [Space]
        [SerializeField]
        private ScenesController scenesController;

        [SerializeField]
        private PrefabLevelsController prefabLevelsController;

        [SerializeField]
        private PlayerInputController playerInputController;

        [SerializeField]
        private UIControllerBase userInterface;

        [SerializeField]
        private CameraController camerasController;

        [SerializeField]
        private ParticlesController particlesController;

        [SerializeField]
        private SoundController soundController;

        [SerializeField]
        private VibrationController vibrationController;

        [Header("CustomComponents")]
        [Space]
        [SerializeField]
        private string customMessage = "Is it custom baby?";

        //[Header("Debugging")]
        //[Space]
        //[SerializeField]
        //private int levelIndexToTest;

        //public void DebugLogBuild(string log)
        //{
        //    userInterface.DebugLogBuild(log);
        //}

        #region SettingsAlternations

        private bool isHaptic;
        public void AlternateHaptic()
        {
            isHaptic = !isHaptic;
            PlayerPrefs.SetInt("Haptic", isHaptic ? 0 : 1);
            vibrationController.IsAllowedToVibrate = isHaptic;
            vibrationController.Vibrate();
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

        // ========= End Generals =========


        // In Custom Code section you add methods that are needed for your game.

        // ========= Custom Code =========

        // In this region a level data is passed to GameManager,
        // which itself passes it to necessary classes/objects.
        #region Level Data HandOver

        // ========= Level Data HandOver =========

        private Player player;
        public void AssignPlayerInstance(Player playerArg)
        {
            player = playerArg;
        }

        private List<Amorph> amorphs;
        public void AssignAmorphsParent(Transform amorphsParent)
        {
            var array = amorphsParent.GetComponentsInChildren<Amorph>();
            amorphs = new List<Amorph>(array);
        }

        // -------- Uncomment what you need --------

        // ========= End Levels HandOver =========

        #endregion

        // In this region methods that are needed
        // during level should be defined and used.
        #region Gameplay
        public void StartGame()
        {
            float angleDelta = 2 * Mathf.PI / amorphs.Count * Mathf.Rad2Deg;
            for (int i = 0; i < amorphs.Count; i++)
            {
                float startingAngle = i * angleDelta;
                amorphs[i].StartRotatingAroundPlayer(player.transform, startingAngle);
            }
        }

        public void UpdatePlayerMovement(float inputX, float inputZ)
        {
            player.UpdateMovementInput(inputX, inputZ);
        }

        public void RotateAmorphs()
        {
            foreach (Amorph a in amorphs)
            {
                a.UpdateRotateAround();
            }
        }
        #endregion

        #region Impressions
        public void PlaySound()
        {
            soundController.StartPlayingSoundFromSource1();
        }

        public void PlayParticles(Vector3 position)
        {
            particlesController.PlayParticles(position);
        }
        #endregion
    }
}


