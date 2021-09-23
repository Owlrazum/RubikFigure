using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using DG.Tweening;

using GeneralTemplate;

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
        }

        [SerializeField]
        private GameObject multiSceneParent;

        private void Start()
        {
            Vibration.Init();
            DontDestroyOnLoad(multiSceneParent);
        }

        private CamerasController camerasController;
        private UIController userInterface;
        private ScenesController scenesController;

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

        public void EndGame(GameResult result)
        {
            userInterface.ProcessGameEnd(result);

            DOTween.KillAll();

            player.ProcessGameEnd(result);

            //result = GameResult.Defeat;

            if (result == GameResult.Win)
            {
                StartVibration();
                scenesController.StartLoadingNextScene();
            }
            else
            {
                StartVibration();
                scenesController.StartReloadingCurrentScene(multiSceneParent);
            }
        }

        public void ProcessNextLevelButtonDown()
        {
            scenesController.FinishLoadingScene();
        }

        private void StartVibration()
        {
            Vibration.Vibrate(200);
        }
    }

}
