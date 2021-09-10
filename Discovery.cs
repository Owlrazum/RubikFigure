using UnityEngine;

namespace Discovery
{
    public static class Save
    {
        public static void SetBool(string key, bool value)
        {
            PlayerPrefs.SetString(key, value.ToString());
        }
        public static bool GetBool(string key)
        {
            if(PlayerPrefs.GetString(key) == "true") return true;
            else return false;
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
        public static void SetObject(string key, object value)
        {
            string json = JsonUtility.ToJson(value);
            PlayerPrefs.SetString(key, value.ToString());
        }
        public static object GetObject(string key)
        {
            return PlayerPrefs.GetString(key);
        }
    }


    namespace Extension
    {   
        public static class TransformExtension
        {
            public static void SetPosition(this Transform transform, Vector3 position)
            {
                transform.position = position;
            }

            public static void SetPosition(this Transform transform, float x, float y)
            {
                transform.position = new Vector2(x, y);
            }

            public static void SetPosition(this Transform transform, float x, float y, float z)
            {
                transform.position = new Vector3(x, y, z);
            }

            public static void SetRotation(this Transform transform, Vector3 rotation)
            {
                transform.rotation = Quaternion.Euler(rotation);
            }

            public static void SetRotation(this Transform transform, float x, float y, float z)
            {
                transform.rotation = Quaternion.Euler(x, y, z);
            }

            public static void SetRotX(this Transform transform, float x)
            {
                transform.rotation = Quaternion.Euler(x, transform.rotation.y, transform.rotation.z);
            }

            public static void SetRotY(this Transform transform, float y)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.x, y, transform.rotation.z);
            }

            public static void SetRotZ(this Transform transform, float z)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, z);
            }

            public static void SetPosX(this Transform transform, float x)
            {
                transform.position = new Vector3(x, transform.position.y, transform.position.z);
            }

            public static void SetPosY(this Transform transform, float y)
            {
                transform.position = new Vector3(transform.position.x, y, transform.position.z);
            }

            public static void SetPosZ(this Transform transform, float z)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y, z);
            }

            public static void SetScale(this Transform transform, Vector3 scale)
            {
                transform.localScale = scale;
            }

            public static void SetScale(this Transform transform, float x, float y, float z)
            {
                transform.localScale = new Vector3(x, y, z);
            }

            public static void SetScale(this Transform transform, Vector2 scale)
            {
                transform.localScale = scale;
            }

            public static void SetScale(this Transform transform, float x, float y)
            {
                transform.localScale = new Vector2(x, y);
            }

            public static void SetScaleX(this Transform transform, float x)
            {
                transform.localScale = new Vector3(x, transform.localScale.y, transform.localScale.z);
            }

            public static void SetScaleY(this Transform transform, float y)
            {
                transform.localScale = new Vector3(transform.localScale.x, y, transform.localScale.z);
            }

            public static void SetScaleZ(this Transform transform, float z)
            {
                transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, z);
            }

            public static float GetPosX(this Transform transform)
            {
                return transform.position.x;
            }
            public static float GetPosY(this Transform transform)
            {
                return transform.position.y;
            }
            public static float GetPosZ(this Transform transform)
            {
                return transform.position.z;
            }

            public static float GetRotX(this Transform transform)
            {
                return transform.rotation.x;
            }

            public static float GetRotY(this Transform transform)
            {
                return transform.rotation.y;
            }

            public static float GetRotZ(this Transform transform)
            {
                return transform.rotation.z;
            }
        }

        public static class ColorExtension
        {
            public static void SetR(this Color color, float r = 1)
            {
                color = new Color(r, color.g, color.b, color.a);
            }

            public static void SetG(this Color color, float g = 1)
            {
                color = new Color(color.r, g, color.b, color.a);
            }

            public static void SetB(this Color color, float b = 1)
            {
                color = new Color(color.r, color.g, b, color.a);
            }

            public static void SetAlpha(this Color color, float a = 1)
            {
                color = new Color(color.r, color.g, color.b, a);
            }

            public static void SetColor(this Color color, float r = 1, float g = 1, float b = 1, float a = 1)
            {
                color = new Color(r, g, b, a);
            }
        }
    }
}