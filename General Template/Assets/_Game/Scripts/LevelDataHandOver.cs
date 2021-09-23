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
        private Transform virtualCamerasParent;

        [SerializeField]
        private Transform enemyCamerasParent;


        private void Start()
        {
            GameManager.Singleton.AssignPlayerInstance(player);
            GameManager.Singleton.AssignEnemiesInstances(enemyCamerasParent);
            GameManager.Singleton.AssignVirtualCamerasParent(virtualCamerasParent);
        }
    }
}
