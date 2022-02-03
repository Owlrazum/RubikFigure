using System.Collections;
using UnityEngine;

/// <summary>
/// A layerMask for visual modification needs to be specified
/// </summary>
public class FieldOfFiewVisual : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    [Header("Field Parameters")]
    [Space]
    [SerializeField]
    [Tooltip("In degrees")]
    private float angle;
    [SerializeField]
    private int segmentsCount;
    [SerializeField]
    private float viewDistance;

    [SerializeField]
    private LayerMask raysCollisionMask;

    private float totalAngle;

    private bool isActive = true;
    private bool isCleared = false;

    private Vector3[] baseVertices;
    private int[] baseIndices;

    private Mesh baseMesh;
    private Mesh changingMesh;

    private Vector3[] changingVertices;

    private void Awake()
    {
        totalAngle = angle * Mathf.Deg2Rad;

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        GeneralEventsContainer.Initialization += OnInitialization;
    }

    private void OnDisable()
    {
        GeneralEventsContainer.Initialization -= OnInitialization;
    }

    private void OnInitialization()
    {
        GenerateBaseMesh();
        meshRenderer.enabled = false;
    }

    #region Generation
    private void GenerateBaseMesh()
    {
        baseVertices = ComputeBaseVertices();
        baseIndices = ComputeBaseIndices();

        baseMesh = new Mesh();

        changingMesh = new Mesh();
        changingMesh.MarkDynamic();
        changingVertices = new Vector3[baseVertices.Length];
        baseVertices.CopyTo(changingVertices, 0);

        baseMesh.vertices = baseVertices;
        baseMesh.triangles = baseIndices;

        changingMesh.vertices = changingVertices;
        changingMesh.triangles = baseIndices;

        meshFilter.mesh = changingMesh;
    }

    private Vector3[] ComputeBaseVertices()
    {
        Vector3[] vertices = new Vector3[segmentsCount + 2];
        int vertexIndex = 0;
        vertices[vertexIndex++] = new Vector3(0, 0, 0);
        float currentAngle = Mathf.PI / 2 - totalAngle / 2;
        float angleDelta = totalAngle / segmentsCount;
        for (int i = 0; i < segmentsCount + 1; i++)
        {
            Vector3 pos = new Vector3(Mathf.Cos(currentAngle), 0, Mathf.Sin(currentAngle));
            pos *= viewDistance;
            vertices[vertexIndex++] = pos;
            currentAngle += angleDelta;
        }
        return vertices;
    }

    private int[] ComputeBaseIndices()
    {
        int[] indices = new int[segmentsCount * 3];
        int vertexCount = segmentsCount + 2;
        int centerIndex = 0;
        int left = centerIndex + 1;
        int right = centerIndex + 2;
        for (int i = 0; i < segmentsCount * 3; i += 3)
        {
            indices[i] = centerIndex;
            indices[i + 1] = right++;
            indices[i + 2] = left++;
        }
        return indices;
    }
    #endregion

    /// <summary>
    /// Likely needs to be renamed depending on context
    /// </summary>
    private IEnumerator Animation()
    {
        while (true)
        {
            for (int i = 1; i < baseVertices.Length; i++)
            {
                Vector3 localDir = baseVertices[i].normalized;
                Vector3 worldDir = transform.TransformDirection(localDir);
                Ray ray = new Ray(transform.position, worldDir);
                if (Physics.Raycast(ray, out RaycastHit rayHit, viewDistance, 
                    raysCollisionMask,
                    QueryTriggerInteraction.Collide))
                {
                    Vector3 worldPos = rayHit.point;
                    Vector3 localPos = transform.InverseTransformPoint(worldPos);
                    changingVertices[i] = localPos;
                }
                else
                {
                    changingVertices[i] = baseVertices[i];
                }
            }
            changingMesh.vertices = changingVertices;
            yield return null;
        }
    }
}