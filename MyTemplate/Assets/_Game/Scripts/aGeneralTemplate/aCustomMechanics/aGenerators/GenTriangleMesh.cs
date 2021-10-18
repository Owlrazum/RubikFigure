using UnityEngine;
using UnityEditor;

namespace Generators
{
    [RequireComponent(typeof(MeshFilter))]
    public class GenTriangleMesh : MonoBehaviour
    {
        [SerializeField]
        private float radius = 1f;
        [SerializeField]
        private float height = 0.5f;

        //TODO: There are problems with how material is displayed. Normals?
        
        void Start()
        {
            Mesh mesh = new Mesh();
            Vector3[] triangleVertices = new Vector3[6];
            Vector2[] uvs = new Vector2[6];
            for (int j = 0; j < 2; j++)
            {
                for (int i = 0; i < 3; i++)
                {
                    float angle = Mathf.PI / 2 - i * 2 * Mathf.PI / 3;
                    Vector3 pos = new Vector3(Mathf.Cos(angle), j * height, Mathf.Sin(angle));
                    pos *= radius;
                    triangleVertices[i + j * 3] = pos;
                }
            }
            uvs[0] = new Vector2(0.5f, 1);
            uvs[1] = new Vector2(1, 0);
            uvs[2] = new Vector2(0, 0);

            uvs[3] = new Vector2(0.5f, 1);
            uvs[4] = new Vector2(1, 0);
            uvs[5] = new Vector2(0, 0);

            mesh.vertices = triangleVertices;
            int[] triangleTriangles = { 3, 4, 5, 0, 2, 1, 0, 3, 2, 3, 5, 2, 3, 0, 1, 3, 1, 4, 2, 5, 1, 5, 4, 1 };
            mesh.triangles = triangleTriangles;
            mesh.RecalculateNormals();
            mesh.RecalculateTangents();
            mesh.uv = uvs;

            MeshFilter meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = mesh;

            AssetDatabase.CreateAsset(mesh, "Assets/_Game/Meshes/Triangle.asset");
            AssetDatabase.SaveAssets();
        }
    }
}

