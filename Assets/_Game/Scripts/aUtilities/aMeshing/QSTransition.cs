using Unity.Mathematics;
using Unity.Collections;

using static QSTransSegment;

namespace Orazum.Meshing
{
    // QS - quad strip
    public struct QSTransition
    {
        private NativeArray<QSTransSegment>.ReadOnly _transitionSegments;
        private QuadStripBuilderVertexData _quadStripBuilder;

        public QSTransition(
            ref NativeArray<VertexData> vertices,
            ref NativeArray<short> indices
        )
        {
            _transitionSegments = new NativeArray<QSTransSegment>.ReadOnly();
            _quadStripBuilder = new QuadStripBuilderVertexData(vertices, indices, float3x2.zero);
        }

        public void AssignTransitionData(
            NativeArray<QSTransSegment>.ReadOnly transitionSegments,
            in float3x2 normalAndUV
        )
        {
            _transitionSegments = transitionSegments;
            _quadStripBuilder.SetNormalAndUV(normalAndUV);
        }

        public void UpdateWithLerpPos(float globalLerpParam, ref MeshBuffersIndexers buffersData)
        {
            for (int i = 0; i < _transitionSegments.Length; i++)
            {
                QSTransSegment segment = _transitionSegments[i];
                for (int j = 0; j < segment.FillDataLength; j++)
                {
                    QSTransSegFillData fillData = segment[j];

                    float2 lerpRange = fillData.LerpRange;
                    if (globalLerpParam >= lerpRange.x && globalLerpParam <= lerpRange.y)
                    {
                        float3x2 start = segment.StartLineSegment;
                        float3x2 end = segment.EndLineSegment;

                        if (fillData.QuadType == QuadConstructType.NewQuadStartToEnd ||
                            fillData.QuadType == QuadConstructType.ContinueQuadStartToEnd)
                        {
                            if (fillData.QuadType == QuadConstructType.NewQuadStartToEnd)
                            { 
                                _quadStripBuilder.Start(start, ref buffersData);
                            }
                            _quadStripBuilder.Continue(end, ref buffersData);
                        }
                        else
                        { 
                            float localLerpParam = math.unlerp(lerpRange.x, lerpRange.y, globalLerpParam);
                            float3x2 middle = new float3x2(
                                math.lerp(start[0], end[0], localLerpParam),
                                math.lerp(start[1], end[1], localLerpParam)
                            ); 
                            if (fillData.QuadType == QuadConstructType.NewQuadFromStart)
                            { 
                                _quadStripBuilder.Start(start, ref buffersData);
                                _quadStripBuilder.Continue(middle, ref buffersData);
                            }
                            else if (fillData.QuadType == QuadConstructType.NewQuadToEnd)
                            {
                                _quadStripBuilder.Start(middle, ref buffersData);
                                _quadStripBuilder.Continue(end, ref buffersData);
                            }
                            else if (fillData.QuadType == QuadConstructType.ContinueQuadFromStart)
                            {
                                _quadStripBuilder.Continue(middle, ref buffersData);
                            }
                            else
                            {
                                throw new System.ArgumentOutOfRangeException("Unknown QSTransSegment.ConstructType");
                            }
                        }
                    }
                }
            }
        }
    }
}