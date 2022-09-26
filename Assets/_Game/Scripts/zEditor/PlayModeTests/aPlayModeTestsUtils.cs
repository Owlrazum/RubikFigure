using System.Collections;

using Unity.Mathematics;

using UnityEngine;

public static class PlayModeTestsUtils
{
    public static void CreateMeshDummy(out MeshFilter meshContainer)
    { 
        GameObject gb = new GameObject("testQuadStrip", typeof(MeshFilter), typeof(MeshRenderer));
        var renderer = gb.GetComponent<MeshRenderer>();
        Material testMaterial = Resources.Load("TestMaterial") as Material;
        renderer.material = testMaterial;

        meshContainer = gb.GetComponent<MeshFilter>();
    }

    public static void CreateCamera(float3 pos, float3 forward, float3 up)
    {
        GameObject gb = new GameObject("Camera", typeof(Camera));
        gb.transform.position = pos;
        gb.transform.rotation = quaternion.LookRotationSafe(forward, up);
    }

    public static void CreateLight(float3 forward, float3 up)
    {
        GameObject gb = new GameObject("Light", typeof(Light));
        Light light = gb.GetComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 2;
        gb.transform.position = new float3(0, -10, 0);
        gb.transform.rotation = quaternion.LookRotationSafe(forward, up);
    }

    private static MonoBehaviour runner;
    public static void StartCoroutine(IEnumerator coroutine)
    {
        if (runner == null)
        {
            GameObject gb = new GameObject("CoroutineRunner", typeof(CoroutineRunner));
            runner = gb.GetComponent<CoroutineRunner>();
        }
        
        runner.StartCoroutine(coroutine);
    }

    private class CoroutineRunner : MonoBehaviour
    { 

    }
}