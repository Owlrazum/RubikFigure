using Unity.Mathematics;

public static class WheelMeshGenUtilities
{
    // public static WheelSegmentVertices GenerateVertexList(
    //     float3 currentRay, 
    //     float3 nextRay, 
    //     float2 uv,
    //     float currentRadius,
    //     float nextRadius)
    // {
    //     WheelSegmentVertices vertexList = new WheelSegmentVertices();
    //     float3 up = new float3(0, 1, 0);
    //     float3 bot = new float3(0, -1, 0);
        
    //     VertexData bv1 = new VertexData();
    //     bv1.position = currentRay * currentRadius;
    //     bv1.normal = bot;
    //     bv1.uv = uv;
    //     vertexList[0] = bv1;

    //     VertexData bv2 = new VertexData();
    //     bv2.position = nextRay * currentRadius;
    //     bv2.normal = bot;
    //     bv2.uv = uv;
    //     vertexList[1] = bv2;

    //     VertexData bv3 = new VertexData();
    //     bv3.position = currentRay * nextRadius;
    //     bv3.normal = bot;
    //     bv3.uv = uv;
    //     vertexList[2] = bv3;

    //     VertexData bv4 = new VertexData();
    //     bv4.position = currentRay * nextRay;
    //     bv4.normal = bot;
    //     bv4.uv = uv;
    //     vertexList[3] = bv4;

    //     VertexData bv5 = new VertexData();
    //     bv5.position = nextRay * currentRadius;
    //     bv5.normal = bot;
    //     bv5.uv = uv;
    //     vertexList[4] = bv5;

    //     VertexData bv6 = new VertexData();
    //     bv6.position = nextRay * nextRadius;
    //     bv6.normal = bot;
    //     bv6.uv = uv;
    //     vertexList[5] = bv6;

    //     return vertexList;
    // }
}