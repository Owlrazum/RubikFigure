using UnityEngine;

namespace Discovery
{
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T _instance;
        private static System.Object _lock = new System.Object();

        public static bool isApplicationQuit;

        public static T Instance
        {
            get
            {
                if (isApplicationQuit)
                    return null;

                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = FindObjectOfType<T>();
                        if (_instance == null)
                        {
                            var singleton = new GameObject("[SINGLETON] " + typeof(T));
                            _instance = singleton.AddComponent<T>();
                            DontDestroyOnLoad(singleton);
                        }
                    }
                    return _instance;
                }
            }
        }

        public virtual void OnDestroy()
        {
            isApplicationQuit = true;
        }
    }
}
