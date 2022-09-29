using Unity.Mathematics;
using Unity.Collections;

namespace Orazum.Meshing
{
    public static class BufferUtils
    {
        public static void MoveBufferIndexer(ref int2 bufferIndexer, int value)
        {
            bufferIndexer.x += bufferIndexer.y;
            bufferIndexer.y = value;
        }

        public static NativeArray<int2> GetFadeInOutTransitionsBufferIndexers(in QuadStripsBuffer quadStripsBuffer)
        {
            NativeArray<int2> toReturn = new(quadStripsBuffer.QuadStripsCount * 2, Allocator.Persistent);
            int2 indexer = int2.zero;
            int toReturnIndexer = 0;
            for (int i = 0; i < quadStripsBuffer.QuadStripsCount; i++)
            {
                QuadStrip qs = quadStripsBuffer.GetQuadStrip(i);
                MoveBufferIndexer(ref indexer, qs.QuadsCount);
                toReturn[toReturnIndexer++] = indexer;
                MoveBufferIndexer(ref indexer, qs.QuadsCount);
                toReturn[toReturnIndexer++] = indexer;
            }

            return toReturn;
        }
    }

    public struct OutInTransitions
    {
        public QS_Transition Out;
        public QS_Transition In;

        public override string ToString()
        {
            return $"FadeOut: {Out.Length}; FadeIn: {In.Length}";
        }
    }
}