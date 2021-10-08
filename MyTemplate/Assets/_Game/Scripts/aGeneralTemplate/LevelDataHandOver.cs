using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    public class LevelDataHandOver : MonoBehaviour
    {
        [SerializeField]
        private Player player;

        [SerializeField]
        private Transform amorphsParent;

        // -------- Uncomment what you need --------

        //[SerializeField]
        //private Transform virtualCamerasParent;

        //[SerializeField]
        //private Transform enemiesParent;


        private void Start()
        {
            GameManager.Singleton.AssignPlayerInstance(player);

            if (amorphsParent != null)
            {
                GameManager.Singleton.AssignAmorphsParent(amorphsParent);
            }

            //if (enemiesParent != null)
            //{
            //    GameManager.Singleton.AssignEnemiesInstances(enemiesParent);
            //}

            //if (virtualCamerasParent != null)
            //{
            //    GameManager.Singleton.AssignVirtualCamerasParent(virtualCamerasParent);
            //}

            GameManager.Singleton.StartGame();
        }
    }
}
