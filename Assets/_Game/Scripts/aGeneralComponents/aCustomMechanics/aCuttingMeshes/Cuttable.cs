using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace CustomMechanincs.CuttingMeshes
{ 
    public class Cuttable : MonoBehaviour
    {
        [SerializeField]
        private Transform _center;

        [SerializeField]
        private Vector2 _cutUV;

        [SerializeField]
        private Material _outerMaterial;

        [SerializeField]
        private Material _innerMaterial;

        [SerializeField]
        private bool _isEnabled = true;

        private Transform _meshTransform;
        private RigidPiece _rigidPiece;

        private Mesh _mesh;
        private MeshFilter _meshFilter;
        private MeshRenderer _meshRenderer;

        private List<List<int>> _relationVertices;
        private Dictionary<int, List<int>> _relationTriangles;
        private Dictionary<int, List<bool>> _triangleIntersection;

        private List<Vector3> _vertices;
        private Vector2[] _uvs;
        private List<int> _triangles;

        private List<int> _originVerticesIndexes;
        private List<Vector3> _originVerticesAdded;
        private List<int> _originTriangles;

        private List<int> _cloneVerticesIndexes;
        private List<Vector3> _cloneVerticesAdded;
        private List<int> _cloneTriangles;

        private int _originInnerTrianglesStartIndex;
        private int _originInnerTrianglesIndexCount;

        private int _cloneInnerTrianglesStartIndex;
        private int _cloneInnerTrianglesIndexCount;

        private void Awake()
        {
            if (!transform.GetChild(0).TryGetComponent(out _rigidPiece))
            {
                Debug.LogError("Rigid piece component is requred for the cuttable");
            }

            _meshTransform = transform.GetChild(0);
            _meshTransform.TryGetComponent(out _meshFilter);
            _meshTransform.TryGetComponent(out _meshRenderer);
            _mesh = _meshFilter.mesh;
            _mesh.MarkDynamic();

            _vertices = new List<Vector3>(_mesh.vertices);
            _triangles = new List<int>(_mesh.triangles);
            _uvs = _mesh.uv;

            _relationVertices = new List<List<int>>();
            _relationTriangles = new Dictionary<int, List<int>>();

            for (int v = 0; v < _vertices.Count; v++)
            {
                _relationVertices.Add(new List<int>());
                int i = 0;
                for (int t = 0; t < _triangles.Count / 3; t++)
                {
                    i = t * 3;
                    if (_triangles[i] == v || _triangles[i + 1] == v || _triangles[i + 2] == v)
                    {
                        _relationVertices[v].Add(t);

                        if (!_relationTriangles.ContainsKey(t))
                        {
                            _relationTriangles.Add(t, new List<int>());
                        }

                        _relationTriangles[t].Add(v);
                        if (_relationTriangles[t].Count > 3)
                        {
                            Debug.LogError("Triangle > 3");
                        }
                    }
                }
            }
        }

        public void SliceMesh(CustomPlane plane)
        {
            if (!_isEnabled)
            {
                return;
            }
            plane.InverseTransform(_meshTransform);

            DetermineTriangleIntersectionData(plane);
            PopulateVertexLists(plane);
            GenerateMeshesAndActivateRigids(plane);  // transforms plane back to world
        }

        private void DetermineTriangleIntersectionData(CustomPlane plane)
        {
            _triangleIntersection = new Dictionary<int, List<bool>>();

            int triangleIndex = -1;

            for (int v = 0; v < _vertices.Count; v++)
            {
                if (plane.CheckSide(_vertices[v]))
                {
                    for (int t = 0; t < _relationVertices[v].Count; t++)
                    {
                        triangleIndex = _relationVertices[v][t];
                        if (!_triangleIntersection.ContainsKey(triangleIndex))
                        {
                            List<bool> falses = new List<bool>();
                            for (int i = 0; i < 3; i++)
                            {
                                falses.Add(false);
                            }
                            _triangleIntersection.Add(triangleIndex, falses);
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            if (_relationTriangles[triangleIndex][i] == v)
                            {
                                _triangleIntersection[triangleIndex][i] = true;
                            }
                        }
                    }
                }
                else
                {
                    for (int t = 0; t < _relationVertices[v].Count; t++)
                    {
                        triangleIndex = _relationVertices[v][t];
                        if (!_triangleIntersection.ContainsKey(triangleIndex))
                        {
                            List<bool> falses = new List<bool>();
                            for (int i = 0; i < 3; i++)
                            {
                                falses.Add(false);
                            }
                            _triangleIntersection.Add(triangleIndex, falses);
                        }
                    }
                }
            }
        }

        private void PopulateVertexLists(CustomPlane plane)
        {
            int originalIndex = 0;
            int cloneIndex = 0;
            int[] newTriangleIndexes = new int[3];
            for (int i = 0; i < 3; i++)
            {
                newTriangleIndexes[i] = -1;
            }

            _originVerticesIndexes = new List<int>();
            _originVerticesAdded = new List<Vector3>();
            _originTriangles = new List<int>();

            _cloneVerticesIndexes = new List<int>();
            _cloneVerticesAdded = new List<Vector3>();
            _cloneTriangles = new List<int>();

            List<Vector3> intersectVertices = new List<Vector3>();

            #region Outer
            for (int t = 0; t < _relationTriangles.Count; t++)
            {
                bool isLeftSide = _triangleIntersection[t][0] && _triangleIntersection[t][1] && _triangleIntersection[t][2];
                bool isRightSide = !_triangleIntersection[t][0] && !_triangleIntersection[t][1] && !_triangleIntersection[t][2];
                if (isLeftSide || isRightSide)
                {
                    int vertexIndex = _relationTriangles[t][0];
                    newTriangleIndexes[0] = vertexIndex;

                    vertexIndex = _relationTriangles[t][1];
                    newTriangleIndexes[1] = vertexIndex;

                    vertexIndex = _relationTriangles[t][2];
                    newTriangleIndexes[2] = vertexIndex;

                    CheckTriangleClockOrderOuter(newTriangleIndexes);

                    if (isLeftSide)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            _originVerticesIndexes.Add(newTriangleIndexes[i]);
                            _originTriangles.Add(originalIndex++);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            _cloneVerticesIndexes.Add(newTriangleIndexes[i]);
                            _cloneTriangles.Add(cloneIndex++);
                        }
                    }

                    continue;
                }

                int trueCount = 0;
                //print(_triangleIntersection[t][0] + " " + _triangleIntersection[t][1] + " " + _triangleIntersection[t][2] + " !");
                foreach (bool side in _triangleIntersection[t])
                {
                    if (side)
                    {
                        trueCount++;
                    }
                }

                if (trueCount != 1 && trueCount != 2)
                {
                    Debug.LogError("Logical error");
                    return;
                }

                int divergingVertex = -1;
                int firstVertex = -1;
                int secondVertex = -1;

                for (int i = 0; i < 3; i++)
                {
                    if (!_triangleIntersection[t][i] && trueCount == 2
                        ||
                        _triangleIntersection[t][i] && trueCount == 1)
                    {
                        divergingVertex = _relationTriangles[t][i];
                    }
                    else
                    {
                        if (firstVertex == -1)
                        {
                            firstVertex = _relationTriangles[t][i];
                        }
                        else
                        {
                            secondVertex = _relationTriangles[t][i];
                        }
                    }
                }


                Vector3? query = plane.GetIntersect(_vertices[divergingVertex], _vertices[firstVertex]);
                //Debug.DrawLine(_vertices[divergingVertex], _vertices[firstVertex], Color.red, 10);
                if (query == null)
                {
                    Debug.LogError("LogicalError");
                    return;
                }
                Vector3 intersectPosFirst = (Vector3)query;
                intersectVertices.Add(intersectPosFirst);


                query = plane.GetIntersect(_vertices[divergingVertex], _vertices[secondVertex]);
                //Debug.DrawLine(_vertices[divergingVertex], _vertices[secondVertex], Color.blue, 10);
                if (query == null)
                {
                    Debug.LogError("LogicalError");
                    return;
                }
                Vector3 intersectPosSecond = (Vector3)query;
                intersectVertices.Add(intersectPosSecond);

                Vector3[] newAddedTriangle = new Vector3[3];

                List<Vector3> twoTrianglesSideVertices;
                List<int> twoTrianglesSideTriangles;
                int twoTrianglesIndex;

                List<Vector3> oneTriangleSideVertices;
                List<int> oneTriangleSideTriangles;
                int oneTriangleIndex;

                if (trueCount == 2)
                {
                    twoTrianglesSideVertices = _originVerticesAdded;
                    twoTrianglesSideTriangles = _originTriangles;
                    twoTrianglesIndex = originalIndex;

                    oneTriangleSideVertices = _cloneVerticesAdded;
                    oneTriangleSideTriangles = _cloneTriangles;
                    oneTriangleIndex = cloneIndex;
                }
                else
                {
                    twoTrianglesSideVertices = _cloneVerticesAdded;
                    twoTrianglesSideTriangles = _cloneTriangles;
                    twoTrianglesIndex = cloneIndex;

                    oneTriangleSideVertices = _originVerticesAdded;
                    oneTriangleSideTriangles = _originTriangles;
                    oneTriangleIndex = originalIndex;
                }

                newAddedTriangle[0] = _vertices[firstVertex];
                newAddedTriangle[1] = _vertices[secondVertex];
                newAddedTriangle[2] = intersectPosFirst;
                CheckTriangleClockOrderOuter(newAddedTriangle);
                //DrawTriangle(newAddedTriangle, Vector3.up * 0.01f, Color.red, 10);
                for (int i = 0; i < 3; i++)
                {
                    twoTrianglesSideVertices.Add(newAddedTriangle[i]);
                    twoTrianglesSideTriangles.Add(twoTrianglesIndex++);
                }

                newAddedTriangle[0] = _vertices[secondVertex];
                newAddedTriangle[1] = intersectPosFirst;
                newAddedTriangle[2] = intersectPosSecond;
                CheckTriangleClockOrderOuter(newAddedTriangle);
                //DrawTriangle(newAddedTriangle, Vector3.zero, Color.green, 10);
                for (int i = 0; i < 3; i++)
                {
                    twoTrianglesSideVertices.Add(newAddedTriangle[i]);
                    twoTrianglesSideTriangles.Add(twoTrianglesIndex++);
                }

                newAddedTriangle[0] = _vertices[divergingVertex];
                newAddedTriangle[1] = intersectPosFirst;
                newAddedTriangle[2] = intersectPosSecond;
                CheckTriangleClockOrderOuter(newAddedTriangle);
                //DrawTriangle(newAddedTriangle, -Vector3.up * 0.01f, Color.blue, 10);
                for (int i = 0; i < 3; i++)
                {
                    oneTriangleSideVertices.Add(newAddedTriangle[i]);
                    oneTriangleSideTriangles.Add(oneTriangleIndex++);
                }

                if (trueCount == 2)
                {
                    originalIndex = twoTrianglesIndex;
                    cloneIndex = oneTriangleIndex;
                }
                else
                {
                    originalIndex = oneTriangleIndex;
                    cloneIndex = twoTrianglesIndex;
                }
            }
            #endregion//Outer

            #region Inner

            _originInnerTrianglesStartIndex = originalIndex;
            _cloneInnerTrianglesStartIndex = cloneIndex;

            Vector3 localCenterPos = Vector3.zero;
            Vector3 intersectCenterPos = plane.GetIntersect(localCenterPos, _meshTransform);
            Vector3[] triangle = new Vector3[3];
            for (int i = 0; i < intersectVertices.Count; i += 2)
            {
                triangle[0] = intersectCenterPos;
                triangle[1] = intersectVertices[i];
                triangle[2] = intersectVertices[i + 1];

                CheckTriangleClockOrderInnerNegativeDot(triangle, plane);

                for (int v = 0; v < 3; v++)
                {
                    _originVerticesAdded.Add(triangle[v]);
                    _originTriangles.Add(originalIndex++);
                }

                triangle[0] = intersectCenterPos;
                triangle[1] = intersectVertices[i];
                triangle[2] = intersectVertices[i + 1];
                CheckTriangleClockOrderInnerPositiveDot(triangle, plane);
                for (int v = 0; v < 3; v++)
                {
                    _cloneVerticesAdded.Add(triangle[v]);
                    _cloneTriangles.Add(cloneIndex++);
                }
            }

            _originInnerTrianglesIndexCount = originalIndex - _originInnerTrianglesStartIndex;
            _cloneInnerTrianglesIndexCount = cloneIndex - _cloneInnerTrianglesStartIndex;
            #endregion//Inner
        }

        private void CheckTriangleClockOrderOuter(int[] triangle)
        {
            if (!IsCorrectClockOrderOuter(
                    _vertices[triangle[0]],
                    _vertices[triangle[1]],
                    _vertices[triangle[2]])
                )
            {
                int temp = triangle[1];
                triangle[1] = triangle[2];
                triangle[2] = temp;
            }
        }

        private void CheckTriangleClockOrderOuter(Vector3[] triangle)
        {
            if (!IsCorrectClockOrderOuter(
                    triangle[0],
                    triangle[1],
                    triangle[2])
                )
            {
                Vector3 temp = triangle[1];
                triangle[1] = triangle[2];
                triangle[2] = temp;
            }
        }

        private void CheckTriangleClockOrderInnerPositiveDot(Vector3[] triangle, CustomPlane plane)
        {
            if (!IsCorrectClockOrderInner(
                    triangle[0],
                    triangle[1],
                    triangle[2],
                    plane)
                )
            {
                Vector3 temp = triangle[1];
                triangle[1] = triangle[2];
                triangle[2] = temp;
            }
        }

        private void CheckTriangleClockOrderInnerNegativeDot(Vector3[] triangle, CustomPlane plane)
        {
            if (IsCorrectClockOrderInner(
                    triangle[0],
                    triangle[1],
                    triangle[2],
                    plane)
                )
            {
                Vector3 temp = triangle[1];
                triangle[1] = triangle[2];
                triangle[2] = temp;
            }
        }

        private void GenerateMeshesAndActivateRigids(CustomPlane plane, bool shouldActivateRigids = true)
        {
            Vector3[] originVerticesFinal = new Vector3[_originVerticesIndexes.Count + _originVerticesAdded.Count];//
            Vector2[] originUV = new Vector2[originVerticesFinal.Length];
            int vertexIndex = 0;

            int outerTrianglesIndexCount = _originVerticesIndexes.Count + _originVerticesAdded.Count - _originInnerTrianglesIndexCount;
            SubMeshDescriptor outerDesc = new SubMeshDescriptor(0, outerTrianglesIndexCount);
            for (int i = 0; i < _originVerticesIndexes.Count; i++)
            {
                originVerticesFinal[vertexIndex] = _vertices[_originVerticesIndexes[i]];
                originUV[vertexIndex] = _uvs[_originVerticesIndexes[i]];
                vertexIndex++;
            }

            SubMeshDescriptor innerDesc = new SubMeshDescriptor(_originInnerTrianglesStartIndex, _originInnerTrianglesIndexCount);
            for (int i = 0; i < _originVerticesAdded.Count; i++)
            {
                originVerticesFinal[vertexIndex] = _originVerticesAdded[i];
                originUV[vertexIndex] = _cutUV;
                vertexIndex++;
            }

            _mesh.Clear();
            _mesh.vertices = originVerticesFinal;
            _mesh.triangles = _originTriangles.ToArray();

            _mesh.subMeshCount = 2;
            _mesh.SetSubMesh(0, outerDesc);
            _mesh.SetSubMesh(1, innerDesc);

            _mesh.uv = originUV;
            _mesh.RecalculateNormals();
            _meshFilter.mesh = _mesh;

            Material[] mats = _meshRenderer.sharedMaterials;
            mats = new Material[2];
            mats[0] = _outerMaterial;
            mats[1] = _innerMaterial;
            _meshRenderer.sharedMaterials = mats;

            Vector3[] cloneVerticesFinal = new Vector3[_cloneVerticesIndexes.Count + _cloneVerticesAdded.Count];
            Vector2[] cloneUV = new Vector2[cloneVerticesFinal.Length];
            vertexIndex = 0;

            outerTrianglesIndexCount = _cloneVerticesIndexes.Count + _cloneVerticesAdded.Count - _cloneInnerTrianglesIndexCount;
            outerDesc = new SubMeshDescriptor(0, outerTrianglesIndexCount);
            for (int i = 0; i < _cloneVerticesIndexes.Count; i++)
            {
                cloneVerticesFinal[vertexIndex] = _vertices[_cloneVerticesIndexes[i]];
                cloneUV[vertexIndex] = _uvs[_cloneVerticesIndexes[i]];
                vertexIndex++;
            }

            innerDesc = new SubMeshDescriptor(_cloneInnerTrianglesStartIndex, _cloneInnerTrianglesIndexCount);
            for (int i = 0; i < _cloneVerticesAdded.Count; i++)
            {
                cloneVerticesFinal[vertexIndex] = _cloneVerticesAdded[i];
                cloneUV[vertexIndex] = _cutUV;
                vertexIndex++;
            }

            var clone = Instantiate(_meshTransform, _meshTransform.position, _meshTransform.rotation, transform);
            clone.TryGetComponent(out MeshFilter cloneFilter);

            Mesh cloneMesh = cloneFilter.mesh;
            cloneMesh.Clear();
            cloneMesh.vertices = cloneVerticesFinal;
            cloneMesh.triangles = _cloneTriangles.ToArray();

            cloneMesh.subMeshCount = 2;
            cloneMesh.SetSubMesh(0, outerDesc);
            cloneMesh.SetSubMesh(1, innerDesc);

            cloneMesh.uv = cloneUV;
            cloneMesh.RecalculateNormals();
            cloneFilter.mesh = cloneMesh;

            clone.TryGetComponent(out MeshRenderer cloneRenderer);
            mats = cloneRenderer.sharedMaterials;
            mats = new Material[2];
            mats[0] = _outerMaterial;
            mats[1] = _innerMaterial;
            cloneRenderer.sharedMaterials = mats;

            plane.Transform(_meshTransform);

            if (shouldActivateRigids)
            {
                clone.TryGetComponent(out RigidPiece cloneRigidPiece);
                if (plane.IsPositiveDot(Vector3.up)) // normal is up
                {
                    _rigidPiece.ActivateRestrained();
                    cloneRigidPiece.ActivateMovable();
                }
                else
                {
                    _rigidPiece.ActivateMovable();
                    cloneRigidPiece.ActivateRestrained();
                }
            }


        }

        private bool IsCorrectClockOrderOuter(Vector3 a, Vector3 b, Vector3 c)
        {
            Vector3 triangleCenter = (a + b + c) / 3;
            Vector3 axisDirection = Vector3.Dot(triangleCenter, Vector3.forward) > 0 ? Vector3.forward : -Vector3.forward;
            Vector3 center = axisDirection * Vector3.Dot(triangleCenter, axisDirection) * 0.9f;
            //SpawnDebugSphere(center);
            //SpawnDebugSphere(triangleCenter);
            Vector3 outwards = triangleCenter - center;
            return Vector3.Dot(outwards, Vector3.Cross(a - b, c - b)) < 0;
        }

        private bool IsCorrectClockOrderInner(Vector3 a, Vector3 b, Vector3 c, CustomPlane plane)
        {
            Vector3 cross = Vector3.Cross(a - b, c - b);
            return plane.IsPositiveDot(cross);
        }

        private void DrawTriangle(Vector3[] triangle, Vector3 offset, Color color, float time)
        {
            Vector3 a = transform.TransformPoint(triangle[0]) + offset;
            Vector3 b = transform.TransformPoint(triangle[1]) + offset;
            Vector3 c = transform.TransformPoint(triangle[2]) + offset;
            Debug.DrawLine(a, b, color, time, false);
            Debug.DrawLine(b, c, color, time, false);
            Debug.DrawLine(c, a, color, time, false);
        }
    }
}
