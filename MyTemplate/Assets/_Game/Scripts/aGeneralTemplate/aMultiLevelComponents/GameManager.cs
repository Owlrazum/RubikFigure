using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using DG.Tweening;

namespace GeneralTemplate
{
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
        }

        [SerializeField]
        private LevelType levelType;
        
        [SerializeField]
        private GameObject multiLevelsParent;

        private void Start()
        {
            Vibration.Init();
            if (levelType == LevelType.Scene)
            {
                DontDestroyOnLoad(multiLevelsParent);
            }
        }

        [Header("GeneralTemplateComponents")]
        [Space]
        [SerializeField]
        private ScenesController scenesController;

        [SerializeField]
        private PrefabLevelsController prefabLevelsController;

        [SerializeField]
        private UIController userInterface;

        [SerializeField]
        private VirtualCamerasController camerasController;

        [SerializeField]
        private ParticlesController particlesController;

        [SerializeField]
        private AudioSource audioSource;

        //[Header("Debugging")]
        //[Space]
        //[SerializeField]
        //private LevelDebugging levelDebugging;


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

        //private List<Enemy> enemies;
        //public void AssignEnemiesInstances(Transform enemiesParent)
        //{
        //    var array = enemiesParent.GetComponentsInChildren<Enemy>();
        //    enemies = new List<Enemy>(array);
        //}


        //public void AssignVirtualCamerasParent(Transform parent)
        //{
        //    camerasController.AssignCameras(parent);
        //}

        // ========= End Levels HandOver =========

        // ========= Custom Code =========


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

        // ========= End Customs =========

        #region SettingsAlternations

        private bool isHaptic;
        public void AlternateHaptic()
        {
            isHaptic = !isHaptic;
            StartVibration();
        }

        private bool hasSound;
        public void AlternateSound()
        {
            hasSound = !hasSound;
            if (hasSound)
            {
                AudioListener.volume = 1;
            }
            else
            {
                AudioListener.volume = 0;
            }
        }

        #endregion

        public void PlaySound(AudioClip clip)
        {
            audioSource.PlayOneShot(clip);
        }

        public void PlayParticles(Vector3 position)
        {
            particlesController.PlayParticles(position);
        }

        public void EndGame(GameResult result)
        {
            userInterface.ProcessGameEnd(result);

            DOTween.KillAll();

            if (player != null)
            {
                player.ProcessGameEnd(result);
            }

            //result = GameResult.Defeat;


            StartVibration();
            if (result == GameResult.Win)
            {
                if (levelType == LevelType.Scene)
                {
                    scenesController.StartLoadingNextScene();
                }
                else
                {
                    prefabLevelsController.StartLoadingNextPrefab();
                }
            }
            else
            {
                if (levelType == LevelType.Scene)
                {
                    scenesController.StartReloadingCurrentScene(multiLevelsParent);
                }
                else
                {
                    prefabLevelsController.StartReloadingCurrentPrefab();
                }
            }
        }

        public void ProcessNextLevelButtonDown()
        {
            if (levelType == LevelType.Scene)
            {
                scenesController.FinishLoadingScene();
            }
            else
            {
                prefabLevelsController.FinishLoadingLevel();
            }
        }

        private void StartVibration()
        {
            Vibration.Vibrate(200);
        }
        
    }
}
