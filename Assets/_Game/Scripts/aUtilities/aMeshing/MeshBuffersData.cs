using Unity.Mathematics;

namespace Orazum.Meshing
{
    /// <summary>
    /// x - vertex, y - index
    /// </summary>
    public struct MeshBuffersData
    {
        public int2 Count;
        public int2 Start;

        public int2 LocalCount;

        public override string ToString()
        {
            return $"{Start.x} {Count.x} {Start.y} {Count.y}";
        }
    }
}