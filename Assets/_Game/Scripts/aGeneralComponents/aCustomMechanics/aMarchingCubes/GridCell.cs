using Unity.Mathematics;

namespace MarchingCubes
{ 
    public struct GridCell
    {
        public float3 LocalPos { get; set; }
        public float Value { get; set; }

    }
}