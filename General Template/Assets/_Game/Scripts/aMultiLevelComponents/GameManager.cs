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

        [Header("MultiLevelsComponentsReferences")]
        [Space]
        [SerializeField]
        private CamerasController camerasController;

        [SerializeField]
        private UIController userInterface;

        [SerializeField]
        private ScenesController scenesController;

        [SerializeField]
        private PrefabLevelsController prefabLevelsController;

        [SerializeField]
        private AudioSource audioSource; 

        [Header("Debugging")]
        [Space]
        [SerializeField]
        private LevelDebugging levelDebugging;

        private List<Enemy> enemies;
        public void AssignEnemiesInstances(Transform enemiesParent)
        {
            var enemiesArray = enemiesParent.
                GetComponentsInChildren<Enemy>(enemiesParent);
            enemies = new List<Enemy>(enemiesArray);
        }

        private Player player;
        public void AssignPlayerInstance(Player playerArg)
        {
            player = playerArg;
        }

        public void AssignVirtualCamerasParent(Transform parent)
        {
            camerasController.AssignCameras(parent);
        }

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
            levelDebugging.gameObject.SetActive(true);
        }

        private void StartVibration()
        {
            Vibration.Vibrate(200);
        }
        
    }
}
