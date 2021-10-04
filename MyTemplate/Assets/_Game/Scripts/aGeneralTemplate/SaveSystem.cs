using UnityEngine;

namespace GeneralTemplate
{
    public static class SaveSystem
    {
        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetInt(key, value == true ? 1 : 0);
        }
        public static bool GetBool(string key, bool _default = false)
        {
            return PlayerPrefs.GetInt(key, _default == true ? 1 : 0) == 1 ? true : false;
        }
        public static void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }
        public static int GetInt(string key)
        {
            return PlayerPrefs.GetInt(key);
        }
        public static void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }
        public static float GetFloat(string key)
        {
            return PlayerPrefs.GetInt(key);
        }
        public static void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
        public static string GetString(string key)
        {
            return PlayerPrefs.GetString(key);
        }
        public static void SetType<T>(string key, T type)
        {
            string json = JsonUtility.ToJson(type);
            PlayerPrefs.SetString(key, json);
        }
        public static T GetType<T>(string key)
        {
            return (JsonUtility.FromJson<T>(PlayerPrefs.GetString(key)));
        }
    }
}
