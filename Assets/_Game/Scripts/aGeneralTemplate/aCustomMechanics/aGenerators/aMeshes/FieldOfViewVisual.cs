using System.Collections;
using UnityEngine;

public class FieldOfViewVisual : MonoBehaviour
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

    private float totalAngle;

    private Vector3[] baseVertices;
    private int[] baseIndices;

    private Mesh changingMesh;
    private Vector3[] changingVertices;

    private void Awake()
    {
        totalAngle = angle * Mathf.Deg2Rad;

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        GeneralEventsContainer.LevelLoaded += OnLevelPreparation;

        //OnLevelPreparation(null);
    }

    private void OnDestroy()
    { 
        GeneralEventsContainer.LevelLoaded -= OnLevelPreparation;
    }

    private void OnLevelPreparation()
    { 
        GenerateBaseMesh();
    }

    private void GenerateBaseMesh()
    {
        baseVertices = ComputeBaseVertices();
        baseIndices = ComputeBaseIndices();

        changingMesh = new Mesh();
        changingMesh.MarkDynamic();
        changingVertices = new Vector3[baseVertices.Length];
        baseVertices.CopyTo(changingVertices, 0);

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

    public void StartCheckingCollisions()
    {
        StartCoroutine(CheckingCollisions());
    }

    private IEnumerator CheckingCollisions()
    {
        while (true)
        {
            for (int i = 1; i < baseVertices.Length; i++)
            {
                Vector3 localDir = baseVertices[i].normalized;
                Vector3 worldDir = transform.TransformDirection(localDir);
                Ray ray = new Ray(transform.position, worldDir);
                if (Physics.Raycast(ray, out RaycastHit rayHit, viewDistance, LayerMask.GetMask("IllusionRaycast"), 
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


//private Vector3[] ComputeRoundedNormals()
// {
//     Vector3[] normals = new Vector3[segmentsCount + 2];
//     int normalIndex = 0;
//     normals[normalIndex++] = -Vector3.forward;
//     Quaternion leftMostRot = Quaternion.Euler(-Vector3.forward * 90);
//     Quaternion rightMostRot = Quaternion.Euler(Vector3.forward * 90);
//     float lerpParam = 0;
//     for (int i = 0; i < segmentsCount + 1; i++)
//     {
//         if (i != 0)
//         { 
//             lerpParam += 1.0f / segmentsCount;
//         }
//         Quaternion currentRot = Quaternion.Slerp(leftMostRot, rightMostRot, lerpParam);
//         Vector3 normal = currentRot * Vector3.up;
//         Debug.DrawRay(baseVertices[i + 1], normal * 100, Color.red, 100, false);

//         normals[normalIndex++] = normal;
//         print("lerpParam " + lerpParam);
//     }
//     return normals;
// }