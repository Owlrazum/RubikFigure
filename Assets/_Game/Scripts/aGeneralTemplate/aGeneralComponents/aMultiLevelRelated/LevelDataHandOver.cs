using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GeneralTemplate
{
    /// <summary>
    /// Everything you need to know about level may go here.
    /// </summary>
    [System.Serializable]
    public class LevelData
    {
        public LevelData()
        { 

        }
    }

    public class LevelDataHandOver : MonoBehaviour
    {
        [SerializeField]
        private LevelData levelData;

        private void Start()
        {
            GeneralEventsContainer.LevelLoaded?.Invoke(levelData);
        }
    }
}
