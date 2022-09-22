using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using Orazum.Collections;
using static Orazum.Math.LineSegmentUtilities;

namespace Orazum.Meshing
{ 
    // QS - quad strip
    public struct QS_Transition
    {
        private NativeArray<QST_Segment> _transSegs;
        private NativeArray<QST_Segment>.ReadOnly _transSegsReadOnly;
        public QS_Transition(NativeArray<QST_Segment> persistentAllocation)
        {
            _transSegs = persistentAllocation;
            _transSegsReadOnly = persistentAllocation.AsReadOnly();
        }

        public int Length { get { return _transSegs.Length; } }
        public QST_Segment this[int index]
        {
            get
            {
                return _transSegsReadOnly[index];
            }
        }
        public bool IsCreated { get { return _transSegs.IsCreated; } }

        public void GetSubTransition(int2 indexer, out QS_Transition subTransition)
        {
            subTransition = new QS_Transition(_transSegs.GetSubArray(indexer.x, indexer.y));
        }

        public QS_Transition GetSubTransition(int2 indexer)
        {
            return new QS_Transition(_transSegs.GetSubArray(indexer.x, indexer.y));
        }

        // In current setup transitions are disposed only if they are concatenated
        public void DisposeConcatenation()
        {
            _transSegs.Dispose();
        }

        public void DisposeConcatenationIfNeeded()
        {
            CollectionUtilities.DisposeIfNeeded(_transSegs);
        }

        public static QS_Transition Concatenate(
            QS_Transition t1,
            QS_Transition t2,
            NativeArray<QST_Segment> buffer
        )
        {
            Assert.IsTrue(buffer.Length == t1.Length + t2.Length);
            int bufferIndexer = 0;
            for (int i = 0; i < t1.Length; i++)
            {
                buffer[bufferIndexer++] = t1[i];
            }

            for (int i = 0; i < t2.Length; i++)
            {
                buffer[bufferIndexer++] = t2[i];
            }
            return new QS_Transition(buffer);
        }

        public static NativeArray<QST_Segment> PrepareConcatenationBuffer(
            QS_Transition t1,
            QS_Transition t2,
            Allocator allocator
        )
        {
            NativeArray<QST_Segment> buffer = new NativeArray<QST_Segment>(t1.Length + t2.Length, allocator);
            return buffer;
        }

        public void DebugTransition(Color c1, Color c2, float duration, float3 f1 = default, float3 f2 = default)
        {
            for (int i = 0; i < Length; i++)
            {
                var s = _transSegs[i];
                float3x2 s1 = s.StartLineSegment;
                float3x2 s2 = s.EndLineSegment;
                float3 f;
                if (i == 0)
                {
                    f = f1;
                }
                else 
                {
                    f = f2;
                }
                Debug.DrawLine(s2[0] + f, s2[1] + f, c2, duration);
                Debug.DrawLine(s1[0] + f, s1[1] + f, c1, duration);
            }
        }

        public override string ToString()
        {
            return $"{Length} transition segments\n";
        }
    }
}