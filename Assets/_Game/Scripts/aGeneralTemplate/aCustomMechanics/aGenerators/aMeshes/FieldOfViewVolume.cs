using UnityEngine;

public class FieldOfViewVolume : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    [SerializeField]
    private int segmentsCount;
    [SerializeField]
    private float angleDeg;
    [SerializeField]
    private float viewDistance;

    private const float HEIGHT = 0.25f;
    private float totalAngleRad;

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();

        totalAngleRad = angleDeg * Mathf.Deg2Rad;

        GeneralEventsContainer.Initialization += OnInitialization;
    }

    private void OnDisable()
    {
        GeneralEventsContainer.Initialization -= OnInitialization;
    }

    private void OnInitialization()
    { 
        GenerateBaseMesh();
    }

    #region Generation
    private void GenerateBaseMesh()
    {
        var baseVertices = ComputeBaseVertices();
        var baseIndices = ComputeBaseIndices();

        Mesh mesh = new Mesh();

        mesh.vertices = baseVertices;
        mesh.triangles = baseIndices;

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    private Vector3[] ComputeBaseVertices()
    {
        Vector3[] vertices = new Vector3[(segmentsCount + 2) * 2];

        //Bottom vertices
        int vertexIndex = 0;
        vertices[vertexIndex++] = new Vector3(0, 0, 0);
        float currentAngleRad = Mathf.PI / 2 - totalAngleRad / 2;
        float angleDeltaRad = totalAngleRad / segmentsCount;
        for (int i = 0; i < segmentsCount + 1; i++)
        {
            Vector3 pos = new Vector3(Mathf.Cos(currentAngleRad), 0, Mathf.Sin(currentAngleRad));
            pos *= viewDistance;
            vertices[vertexIndex++] = pos;
            currentAngleRad += angleDeltaRad;
        }

        //Upper vertices
        vertices[vertexIndex++] = new Vector3(0, HEIGHT, 0);
        currentAngleRad = Mathf.PI / 2 - totalAngleRad / 2;
        for (int i = 0; i < segmentsCount + 1; i++)
        {
            Vector3 pos = new Vector3(Mathf.Cos(currentAngleRad), HEIGHT, Mathf.Sin(currentAngleRad));
            pos.x *= viewDistance;
            pos.z *= viewDistance;
            vertices[vertexIndex++] = pos;
            currentAngleRad += angleDeltaRad;
        }

        return vertices;
    }

    private int[] ComputeBaseIndices()
    {
        int[] indices = new int[segmentsCount * 3 * 2 + 12 + segmentsCount * 6];

        int indexIndex = 0;

        //BottomIndices
        int centerIndex = 0;
        int left = centerIndex + 1;
        int right = centerIndex + 2;
        for (int i = 0; i < segmentsCount * 3; i += 3)
        {
            indices[indexIndex++] = centerIndex;
            indices[indexIndex++] = left++;
            indices[indexIndex++] = right++;
        }

        //UpperIndices
        centerIndex = segmentsCount + 1 + 1;
        left = centerIndex + 2;
        right = centerIndex + 1;
        for (int i = 0; i < segmentsCount * 3; i += 3)
        {
            indices[indexIndex++] = centerIndex;
            indices[indexIndex++] = left++;
            indices[indexIndex++] = right++;
        }

        //SideIndexes
        indices[indexIndex++] = 0;
        indices[indexIndex++] = centerIndex;
        indices[indexIndex++] = 1;

        indices[indexIndex++] = 1;
        indices[indexIndex++] = centerIndex;
        indices[indexIndex++] = 1 + centerIndex;

        indices[indexIndex++] = centerIndex;
        indices[indexIndex++] = 0;
        indices[indexIndex++] = segmentsCount + 1;

        indices[indexIndex++] = centerIndex;
        indices[indexIndex++] = segmentsCount + 1;
        indices[indexIndex++] = segmentsCount + 1 + centerIndex;

        for (int i = 0; i < segmentsCount; i++)
        {
            indices[indexIndex++] = i + 1;
            indices[indexIndex++] = i + 1 + centerIndex;
            indices[indexIndex++] = i + 2;

            indices[indexIndex++] = i + 2;
            indices[indexIndex++] = i + 1 + centerIndex;
            indices[indexIndex++] = i + 2 + centerIndex;
        }

        return indices;
    }
    #endregion
    
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer(""))
        {
        }
    }

    private void OnTriggerExit(Collider collider)
    {
        if (collider.gameObject.layer == LayerMask.NameToLayer(""))
        {
        }
    }
}

