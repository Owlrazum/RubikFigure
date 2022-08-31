using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

using static QSTransSegment;

namespace Orazum.Meshing
{
    // QS - quad strip
    public struct QSTransition
    {
        private NativeArray<QSTransSegment>.ReadOnly _transitionSegments;
        private QuadStripBuilderVertexData _quadStripBuilder;
        private float _globalLerpParam;

        public QSTransition(
            ref NativeArray<VertexData> vertices,
            ref NativeArray<short> indices
        )
        {
            _transitionSegments = new NativeArray<QSTransSegment>.ReadOnly();
            _quadStripBuilder = new QuadStripBuilderVertexData(vertices, indices, float3x2.zero);
            _globalLerpParam = -1;
        }

        public void AssignTransitionData(
            NativeArray<QSTransSegment>.ReadOnly transitionSegments,
            in float3x2 normalAndUV
        )
        {
            _transitionSegments = transitionSegments;
            _quadStripBuilder.SetNormalAndUV(normalAndUV);
        }

        public void UpdateWithLerpPos(float globalLerpParam, ref MeshBuffersIndexers buffersIndexers)
        {
            _globalLerpParam = globalLerpParam;

            for (int i = 0; i < _transitionSegments.Length; i++)
            {
                QSTransSegment segment = _transitionSegments[i];
                for (int j = 0; j < segment.FillDataLength; j++)
                {
                    ConsiderFillData(in segment, segment[j], ref buffersIndexers);
                }
            }
        }

        private void ConsiderFillData(
            in QSTransSegment segment,
            in QSTransSegFillData fillData,
            ref MeshBuffersIndexers buffersIndexers)
        {
            float2 lerpRange = fillData.LerpRange;
            if (_globalLerpParam >= lerpRange.x && _globalLerpParam <= lerpRange.y)
            {
                float3x4 startEndLineSegs = new float3x4(
                    segment.StartLineSegment[0], 
                    segment.StartLineSegment[1], 
                    segment.EndLineSegment[0], 
                    segment.EndLineSegment[1]
                );

                if (fillData.ConstructType == MeshConstructType.Quad)
                {
                    ConstructWithQuadType(
                        fillData.QuadType,
                        in lerpRange,
                        in startEndLineSegs,
                        ref buffersIndexers
                    );
                }
            }
        }

        private void ConstructWithQuadType(
            QuadConstructType quadType,
            in float2 lerpRange,
            in float3x4 startEndLineSegs,
            ref MeshBuffersIndexers buffersIndexers)
        {
            float3x2 start = new float3x2(startEndLineSegs[0], startEndLineSegs[1]);
            float3x2 end = new float3x2(startEndLineSegs[2], startEndLineSegs[3]);

            if (quadType == QuadConstructType.NewQuadStartToEnd ||
                quadType == QuadConstructType.ContinueQuadStartToEnd)
            {
                if (quadType == QuadConstructType.NewQuadStartToEnd)
                {
                    _quadStripBuilder.Start(start, ref buffersIndexers);
                }
                _quadStripBuilder.Continue(end, ref buffersIndexers);
            }
            else
            {
                float localLerpParam = math.unlerp(lerpRange.x, lerpRange.y, _globalLerpParam);
                float3x2 middle = new float3x2(
                    math.lerp(start[0], end[0], localLerpParam),
                    math.lerp(start[1], end[1], localLerpParam)
                );
                if (quadType == QuadConstructType.NewQuadFromStart)
                {
                    _quadStripBuilder.Start(start, ref buffersIndexers);
                    _quadStripBuilder.Continue(middle, ref buffersIndexers);
                }
                else if (quadType == QuadConstructType.NewQuadToEnd)
                {
                    _quadStripBuilder.Start(middle, ref buffersIndexers);
                    _quadStripBuilder.Continue(end, ref buffersIndexers);
                }
                else if (quadType == QuadConstructType.ContinueQuadFromStart)
                {
                    _quadStripBuilder.Continue(middle, ref buffersIndexers);
                }
                else
                {
                    throw new System.ArgumentOutOfRangeException("Unknown QSTransSegment.ConstructType");
                }
            }
        }
    }
}