using UnityEngine;

namespace CustomMechanincs.CuttingMeshes
{
    /// <summary>
    /// perhaps a Unity's default plane can be used, not sure. The main differeтce is in Transform method
    /// </summary>
    public class CustomPlane
    {
        private Vector3 _a;
        private Vector3 _b;
        private Vector3 _c;

        private Vector3 _normal;

        private const float EPSILON = 0.0000001f;

        public CustomPlane(Vector3 a, Vector3 b, Vector3 c)
        {
            _a = a;
            _b = b;
            _c = c;

            _normal = Vector3.Cross(_a - _b, _c - _b).normalized;
        }

        public bool CheckSide(Vector3 vertex)
        {
            Vector3 v = _a - vertex;
            return Vector3.Dot(v, _normal) > 0;
        }

        public bool IsPositiveDot(Vector3 vector)
        {
            return Vector3.Dot(vector, _normal) > 0;
        }

        public void Transform(Transform transform)
        {
            _a = transform.TransformPoint(_a);
            _b = transform.TransformPoint(_b);
            _c = transform.TransformPoint(_c);

            _normal = Vector3.Cross(_a - _b, _c - _b).normalized;
        }

        public void InverseTransform(Transform transform)
        {
            _a = transform.InverseTransformPoint(_a);
            _b = transform.InverseTransformPoint(_b);
            _c = transform.InverseTransformPoint(_c);

            _normal = Vector3.Cross(_a - _b, _c - _b).normalized;
        }

        public Vector3? GetIntersect(Vector3 edgeStartWorld, Vector3 edgeEndWorld)
        {
            Vector3 edge = edgeEndWorld - edgeStartWorld;

            float edgeDot = Vector3.Dot(_normal, edge);
            if (Mathf.Abs(edgeDot) > EPSILON)
            {
                float planeDot = Vector3.Dot(_normal, _a);
                float edgeStartDot = Vector3.Dot(_normal, edgeStartWorld);
                float factor = (planeDot - edgeStartDot) / edgeDot;
                if (factor < 0 || factor > 1)
                {
                    return null;
                }

                return edgeStartWorld + edge * factor;
            }
            return null;
        }

        public Vector3 GetIntersect(Vector3 point, Transform transform)
        {
            Vector3 edgeStartWorld = point;

            Vector3 edgeEndWorld = point - _normal * 100;
            Vector3 edge = edgeEndWorld - edgeStartWorld;

            // Debug.DrawLine(transform.TransformPoint(edgeStartWorld), transform.TransformPoint(edgeEndWorld), Color.white, 100, false);
            //Debug.DrawLine(transform.position, transform.position + Vector3.up * 10, Color.white, 100, false);
            // Debug.DrawLine(transform.TransformPoint(edgeStartWorld), transform.TransformPoint(edgeEndWorld + Vector3.forward * 10), Color.green, 100, false);
            // DrawPlane(new Vector3[]{ _a, _b, _c}, Vector3.zero, new Color(1, colorOffset, colorOffset, 1), 100, transform);
            // colorOffset += 0.3f;

            float edgeDot = Vector3.Dot(_normal, edge);
            if (Mathf.Abs(edgeDot) <= EPSILON)
            {
                //Debug.LogWarning("The intersect point is already in the plain");
                return point;
            }
            float planeDot = Vector3.Dot(_normal, _a);
            float edgeStartDot = Vector3.Dot(_normal, edgeStartWorld);
            float factor = (planeDot - edgeStartDot) / edgeDot;

            return edgeStartWorld + edge * factor;
        }

        private void DrawPlane(Vector3[] triangle, Vector3 offset, Color color, float time, Transform transform)
        {
            Vector3 a = transform.TransformPoint(triangle[0]) + offset;
            Vector3 b = transform.TransformPoint(triangle[1]) + offset;
            Vector3 c = transform.TransformPoint(triangle[2]) + offset;
            Debug.DrawLine(a, b, color, time, false);
            Debug.DrawLine(b, c, color, time, false);
            Debug.DrawLine(c, a, color, time, false);

            Vector3 center = (a + b + c) / 3;
            Vector3 normal = transform.TransformVector(_normal);
            Debug.DrawLine(center, center + normal, color, time, false);
        }

        public void DrawIt()
        {
            Debug.DrawLine(_a, _b, Color.red, 10, false);
            Debug.DrawLine(_b, _c, Color.red, 10, false);
            Debug.DrawLine(_c, _a, Color.red, 10, false);
        }
    }
}

