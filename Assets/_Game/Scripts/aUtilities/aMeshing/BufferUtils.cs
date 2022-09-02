using Unity.Mathematics;

namespace Orazum.Meshing
{ 
    public static class BufferUtils
    {
        public static void MoveBufferIndexer(ref int2 bufferIndexer, int value)
        {
            bufferIndexer.x += bufferIndexer.y;
            bufferIndexer.y += value;
        }
    }
}