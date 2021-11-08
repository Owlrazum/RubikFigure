using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    // TODO Decide EndGame method
    // TODO Decide whether ParticlesController should use ObjectPool;

    // Perhaps there is more to be taken from the saw.

    // Tip: There is a confetti plane in the main camera.

    public class GameManager : MonoBehaviour
    {

        // Generally, you should not modify General Code section.

        // ========= General Code =========
        #region SerializedFields, UnityEvent methods, Singleton

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
            userInterface.SetHapticPresentation(isHaptic);
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
        private VirtualCamerasController camerasController;

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

        #endregion

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
            soundController.PlaySoundTurnOn();
        }

        #endregion

        #region LevelEndLogic
        public void EndCurrentLevel()
        {
            userInterface.ProcessLevelEnd();

            vibrationController.Vibrate();
            soundController.PlayWinSound();

            if (levelType == LevelType.Scene)
            {
                scenesController.StartLoadingNextScene();
            }
            else
            {
                prefabLevelsController.StartLoadingNextPrefab();
            }
        }

        public void ProcessNextLevelButtonDown()
        {
            if (levelType == LevelType.Scene)
            {
                SaveSystem.SetInt
                    ("LastLevel", scenesController.GetCurrentLevelIndex());
                scenesController.FinishLoadingScene();
            }
            else
            {
                SaveSystem.SetInt
                    ("LastLevel", prefabLevelsController.GetCurrentLevelIndex());
                prefabLevelsController.FinishLoadingLevel();
            }
        }
        #endregion

        #region OnApplicationQuit & Focus
        /// <summary>
        /// Did not work one time,
        /// therefore LastLevel PlayerPref is writed on each level
        /// </summary>
        private void OnApplicationFocus(bool focus)
        {
            if (!focus)
            {
                if (levelType == LevelType.Scene)
                {
                    SaveSystem.SetInt
                        ("LastLevel", scenesController.GetCurrentLevelIndex());
                }
                else if (levelType == LevelType.Prefab)
                {
                    SaveSystem.SetInt
                        ("LastLevel", prefabLevelsController.GetCurrentLevelIndex());
                }
            }
        }

        private void OnApplicationQuit()
        {
            if (levelType == LevelType.Scene)
            {
                SaveSystem.SetInt
                    ("LastLevel", prefabLevelsController.GetCurrentLevelIndex());
            }
            else if (levelType == LevelType.Prefab)
            {
                SaveSystem.SetInt
                    ("LastLevel", scenesController.GetCurrentLevelIndex());
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

        public void PlaySound()
        {
            soundController.StartPlayingSoundFromSource1();
        }

        public void PlayParticles(Vector3 position)
        {
            particlesController.PlayParticles(position);
        }
        #endregion

        // ========= End Customs =========


        // Commented methods that may or may not used.

        // List of methods:
        // public void EndGame(GameResult result);
        // public void AssignEnemiesInstances(Transform enemiesParent);
        // public void AssignVirtualCamerasParent(Transform camerasParent);
        #region TemplatesForMethods

        //private List<Enemy> enemies;
        //public void AssignEnemiesInstances(Transform enemiesParent)
        //{
        //    var array = enemiesParent.GetComponentsInChildren<Enemy>();
        //    enemies = new List<Enemy>(array);
        //}

        //public void AssignVirtualCamerasParent(Transform camerasParent)
        //{
        //    camerasController.AssignCameras(parent);
        //}

        //public void EndGame(GameResult result)
        //{
        //    userInterface.ProcessGameEnd(result);

        //    if (player != null)
        //    {
        //        player.ProcessGameEnd(result);
        //    }

        //    vibrationController.Vibrate();
        //    soundController.PlayWinSound();

        //    if (result == GameResult.Win)
        //    {
        //        if (levelType == LevelType.Scene)
        //        {
        //            scenesController.StartLoadingNextScene();
        //        }
        //        else
        //        {
        //            prefabLevelsController.StartLoadingNextPrefab();
        //        }
        //    }
        //    else
        //    {
        //        if (levelType == LevelType.Scene)
        //        {
        //            scenesController.StartReloadingCurrentScene(multiLevelsParent);
        //        }
        //        else
        //        {
        //            prefabLevelsController.StartReloadingCurrentPrefab();
        //        }
        //    }
        //}
        #endregion
    }
}


