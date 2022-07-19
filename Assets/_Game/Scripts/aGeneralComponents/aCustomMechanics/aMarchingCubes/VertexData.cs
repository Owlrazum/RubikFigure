using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace MarchingCubes
{ 
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexData
    {
        public float3 position;
        public float3 normal;

        public VertexData(float3 position, float3 normal)
        {
            this.position = position;
            this.normal = normal;
        }
        public static readonly VertexAttributeDescriptor[] VertexBufferMemoryLayout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal)
        };

        public override string ToString()
        {
            return position.ToString();
        }
    }
}
