using System.Collections;

using Unity.Collections;
using Unity.Mathematics;

using UnityEngine;
using UnityEngine.Rendering;

using Orazum.Meshing;

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
        gb.transform.rotation = quaternion.LookRotationSafe(forward, up);
    }

    public static void ApplyMeshBuffers(
        in NativeArray<VertexData> vertices, 
        in NativeArray<short> indices, 
        in MeshFilter meshContainer,
        in MeshBuffersIndexers buffersIndexers)
    { 
        Mesh mesh = meshContainer.mesh;
        mesh.MarkDynamic();

        mesh.SetVertexBufferParams(buffersIndexers.Count.x, VertexData.VertexBufferMemoryLayout);
        mesh.SetIndexBufferParams(buffersIndexers.Count.y, IndexFormat.UInt16);

        mesh.SetVertexBufferData(vertices, buffersIndexers.Start.x, 0, buffersIndexers.Count.x, 0, MeshUpdateFlags.Default);
        mesh.SetIndexBufferData(indices, buffersIndexers.Start.y, 0, buffersIndexers.Count.y, MeshUpdateFlags.Default);

        mesh.subMeshCount = 1;
        SubMeshDescriptor subMesh = new SubMeshDescriptor(
            indexStart: 0,
            indexCount: buffersIndexers.Count.y
        );
        mesh.SetSubMesh(0, subMesh);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
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