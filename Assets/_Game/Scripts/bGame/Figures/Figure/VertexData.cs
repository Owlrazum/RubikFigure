using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
public struct VertexData
{
    public float3 position;
    public float3 normal;
    public float2 uv;

    public VertexData(float3 position, float3 normal, float2 uv)
    {
        this.position = position;
        this.normal = normal;
        this.uv = uv;
    }

    public static readonly VertexAttributeDescriptor[] VertexBufferMemoryLayout =
    {
        new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0),
        new VertexAttributeDescriptor(VertexAttribute.Normal, stream: 0),
        new VertexAttributeDescriptor(
                VertexAttribute.TexCoord0, 
                VertexAttributeFormat.Float32, 
                dimension: 2, 
                stream : 0)
    };

    public static readonly VertexAttributeDescriptor[] PositionBufferMemoryLayout =
    {
        new VertexAttributeDescriptor(VertexAttribute.Position, stream: 0)
    };

    public override string ToString()
    {
        return position.ToString();
    }
}