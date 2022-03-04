using UnityEngine;

/// <summary>
/// Should be used in tandem with FieldOfViewVisualBorder.
/// </summary>
public class FieldOfViewVisualBorder : MonoBehaviour
{
    [SerializeField]
    private float borderLength; 

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer; 

    private Mesh changingMesh;

    private Vector3[] baseVertices;
    private Vector3[] changingVertices;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public void SetActiveRenderer(bool isActive)
    {
        meshRenderer.enabled = isActive;
    }

    public void GenerateBorder(Vector3[] vertices, int[] indices)
    { 
        changingMesh = new Mesh();
        changingMesh.MarkDynamic();

        baseVertices = ExtrudeVertices(vertices);
        changingVertices = new Vector3[baseVertices.Length];
        baseVertices.CopyTo(changingVertices, 0);

        changingMesh.vertices = changingVertices;
        changingMesh.triangles = indices;

        meshFilter.mesh = changingMesh;
    }

    private Vector3[] ExtrudeVertices(Vector3[] vertices)
    {
        Vector3 center = vertices[0];
        for (int i = 1; i < vertices.Length; i++)
        {
            Vector3 dir = (vertices[i] - center).normalized;
            vertices[i] += dir * borderLength;
        }

        vertices[0] -= Vector3.forward * borderLength;
        Vector2 left = new Vector2(
            vertices[vertices.Length - 1].x - vertices[0].x,
            vertices[vertices.Length - 1].z - vertices[0].z
            
        );
        Vector2 right = new Vector2(
            vertices[0].x - vertices[1].x, 
            vertices[0].z - vertices[1].z
        );
        Vector2 extudeLeft = Vector2.Perpendicular(left);
        Vector2 extudeRight = Vector2.Perpendicular(right);

        vertices[1] += (new Vector3(extudeRight.x, 0, extudeRight.y)).normalized * borderLength;
        vertices[vertices.Length - 1] += 
            (new Vector3(extudeLeft.x, 0, extudeLeft.y)).normalized * borderLength;

        return vertices;
    }

    public void UpdateBorderVertex(int index, Vector3 newPos)
    {
        changingVertices[index] = newPos;
    }

    public void ResetBorderVertex(int index)
    {
        changingVertices[index] = baseVertices[index];
    }

    public void UpdateBorderMesh()
    {
        changingMesh.vertices = changingVertices;
    }
}