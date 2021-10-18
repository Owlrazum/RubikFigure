using UnityEngine;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Discovery
{
    public static class Save
    {
        class Bool { public Bool(bool value) { flag = value; } public bool flag; }
        public static void SetBool(string key, bool value)
        {
            Bool b = new Bool(value);
            PlayerPrefs.SetString(key, JsonUtility.ToJson(b));
        }
        public static bool GetBool(string key, bool _default = default)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return JsonUtility.FromJson<Bool>(PlayerPrefs.GetString(key)).flag;
            }
            else return _default;
        }
        class Int { public Int(int value) { flag = value; } public int flag; }
        public static void SetInt(string key, int value)
        {
            Int i = new Int(value);
            PlayerPrefs.SetString(key, JsonUtility.ToJson(i));
        }
        public static int GetInt(string key, int _default = default)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return JsonUtility.FromJson<Int>(PlayerPrefs.GetString(key)).flag;
            }
            else return _default;
        }
        class Float { public Float(float value) { flag = value; } public float flag; }
        public static void SetFloat(string key, float value)
        {
            Float f = new Float(value);
            PlayerPrefs.SetString(key, JsonUtility.ToJson(f));
        }
        public static float GetFloat(string key, float _default = default)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return JsonUtility.FromJson<Float>(PlayerPrefs.GetString(key)).flag;
            }
            else return _default;
        }
        class String { public String(string value) { flag = value; } public string flag; }
        public static void SetString(string key, string value)
        {
            String s = new String(value);
            PlayerPrefs.SetString(key, JsonUtility.ToJson(s));
        }
        public static string GetString(string key, string _default = default)
        {
            if (PlayerPrefs.HasKey(key))
            {
                return JsonUtility.FromJson<String>(PlayerPrefs.GetString(key)).flag;
            }
            else return _default;
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

        public static void Clear()
        {
            PlayerPrefs.DeleteAll();
        }

        public static void Clear(params string[] key)
        {
            for (int i = 0; i < key.Length; i++)
            {
                PlayerPrefs.DeleteKey(key[i]);
            }
        }

        public static bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public static void RenameKey(string oldKey, string newKey)
        {
            if (Save.HasKey(oldKey))
            {
                string json = Save.GetString(oldKey);
                Save.Clear(oldKey);
                Save.SetString(newKey, json);
            }
        }

    }

    public static class SNG
    {
        public static Vector3 RadianToVector2(float radian)
        {
            return new Vector3(Mathf.Cos(radian), 0, Mathf.Sin(radian));
        }

        public static Vector3 DegreeToVector2(float degree)
        {
            return RadianToVector2(degree * Mathf.Deg2Rad);
        }

        public static List<T> Shuffle<T>(List<T> list)
        {
            List<T> list2 = new List<T>();
            int n = list.Count;
            for (int i = 0; i < n; i++)
            {
                int count = list.Count;
                int rand = UnityEngine.Random.Range(0, count);
                list2.Add(list[rand]);
                list.RemoveAt(rand);
            }
            return list2;
        }

        public static Vector3 Reflect(Vector3 inDirection, Vector3 inNormal)
        {
            return (((Vector3)((-2f * Vector3.Dot(inNormal, inDirection)) * inNormal)) + inDirection);
        }
    }

    public enum Axis { x, y, z, xy, yz, xz, xyz }
    public enum ModeAxis { X, Y, Z }

    namespace Extension
    {
        public static class TransformExtension
        {
            public static void SetPosition(this Transform transform, Vector3 position, Axis mode = Axis.xyz)
            {
                switch (mode)
                {
                    case Axis.x:
                        {
                            transform.position = new Vector3(position.x, transform.position.y, transform.position.z);
                        }
                        break;
                    case Axis.y:
                        {
                            transform.position = new Vector3(transform.position.x, position.y, transform.position.z);
                        }
                        break;
                    case Axis.z:
                        {
                            transform.position = new Vector3(transform.position.x, transform.position.y, position.z);
                        }
                        break;
                    case Axis.xy:
                        {
                            transform.position = new Vector3(position.x, position.y, transform.position.z);
                        }
                        break;
                    case Axis.yz:
                        {
                            transform.position = new Vector3(transform.position.x, position.y, position.z);
                        }
                        break;
                    case Axis.xz:
                        {
                            transform.position = new Vector3(position.x, transform.position.y, position.z);
                        }
                        break;
                    case Axis.xyz:
                        {
                            transform.position = position;
                        }
                        break;
                }
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

            public static void MoveTowards(this Transform transform, Vector3 to, float speed)
            {
                transform.position = Vector3.MoveTowards(transform.position, to, speed);
            }

            public static void Lerp(this Transform transform, Vector3 to, float speed)
            {
                transform.position = Vector3.Lerp(transform.position, to, speed);
            }

            public static void RotateTowards(this Transform transform, Quaternion rotation, float speed)
            {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, rotation, speed);
            }

            public static void RotateLerp(this Transform transform, Quaternion rotation, float speed)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, rotation, speed);
            }

            public static void LookRotation(this Transform transform, Vector3 position, ModeAxis mode = ModeAxis.Y)
            {
                switch (mode)
                {
                    case ModeAxis.Y:
                        {
                            transform.rotation = Quaternion.LookRotation(position, Vector3.up);
                        }
                        break;
                    case ModeAxis.Z:
                        {
                            transform.rotation = Quaternion.LookRotation(position, Vector3.forward);
                        }
                        break;
                    case ModeAxis.X:
                        {
                            transform.rotation = Quaternion.LookRotation(position, Vector3.right);
                        }
                        break;
                }
            }



            public static bool Equals(this Transform transform, Vector3 position, Axis mode = Axis.xyz)
            {
                switch (mode)
                {
                    case Axis.x:
                        {
                            if (transform.position.x == position.x) return true;
                        }
                        break;
                    case Axis.y:
                        {
                            if (transform.position.y == position.y) return true;
                        }
                        break;
                    case Axis.z:
                        {
                            if (transform.position.z == position.z) return true;
                        }
                        break;
                    case Axis.xy:
                        {
                            if (transform.position.x == position.x && transform.position.y == position.y) return true;
                        }
                        break;
                    case Axis.yz:
                        {
                            if (transform.position.y == position.y && transform.position.z == position.z) return true;
                        }
                        break;
                    case Axis.xz:
                        {
                            if (transform.position.x == position.x && transform.position.z == position.z) return true;
                        }
                        break;
                    case Axis.xyz:
                        {
                            if (transform.position == position) return true;
                        }
                        break;
                }
                return false;
            }
            public static bool IsDefault(this Transform transform)
            {
                if (transform.position != Vector3.zero && transform.rotation != Quaternion.identity && transform.localScale != Vector3.zero)
                {
                    return false;
                }
                else return true;
            }
        }

        public static class VectorExtension
        {
            public static bool IsDefault(this Vector2 vector2)
            {
                if (vector2 != Vector2.zero)
                {
                    return false;
                }
                else return true;
            }
            public static bool IsDefault(this Vector3 vector3)
            {
                if (vector3 != Vector3.zero)
                {
                    return false;
                }
                else return true;
            }

            public static void Set(this Vector2 vector2, float x, float y)
            {
                vector2 = new Vector2(x, y);
            }
            public static void Set(this Vector3 vector3, float x, float y, float z)
            {
                vector3 = new Vector3(x, y, z);
            }

            public static bool Equals(this Vector3 vector, Vector3 position, Axis mode = Axis.xyz)
            {
                switch (mode)
                {
                    case Axis.x:
                        {
                            if (vector.x == position.x) return true;
                        }
                        break;
                    case Axis.y:
                        {
                            if (vector.y == position.y) return true;
                        }
                        break;
                    case Axis.z:
                        {
                            if (vector.z == position.z) return true;
                        }
                        break;
                    case Axis.xy:
                        {
                            if (vector.x == position.x && vector.y == position.y) return true;
                        }
                        break;
                    case Axis.yz:
                        {
                            if (vector.y == position.y && vector.z == position.z) return true;
                        }
                        break;
                    case Axis.xz:
                        {
                            if (vector.x == position.x && vector.z == position.z) return true;
                        }
                        break;
                    case Axis.xyz:
                        {
                            if (vector == position) return true;
                        }
                        break;
                }
                return false;
            }
        }

        public static class ColorExtension
        {
            public static bool Equals(this Color c, Color color)
            {
                if (c == color) return true;
                else return false;
            }

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
            public static void SetColor(this Color color, Color rgb)
            {
                color = rgb;
            }
        }
    }
}
