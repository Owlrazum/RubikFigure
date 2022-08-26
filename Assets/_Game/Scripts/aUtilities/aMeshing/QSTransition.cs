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
                        float4 start = new float4(segment.StartLineSegment[0], segment.StartLineSegment[1]);
                        float4 end = new float4(segment.EndLineSegment[0], segment.EndLineSegment[1]);

                        if (fillData.ConstructType == QuadConstructType.NewQuadStartToEnd ||
                            fillData.ConstructType == QuadConstructType.ContinueQuadStartToEnd)
                        {
                            if (fillData.ConstructType == QuadConstructType.NewQuadStartToEnd)
                            { 
                                _quadStripBuilder.Start(new float2x2(start.xy, start.zw), ref buffersData);
                            }
                            _quadStripBuilder.Continue(new float2x2(end.xy, end.zw), ref buffersData);
                        }
                        else
                        { 
                            float localLerpParam = math.unlerp(lerpRange.x, lerpRange.y, globalLerpParam);
                            float4 middle = math.lerp(start, end, localLerpParam);
                            if (fillData.ConstructType == QuadConstructType.NewQuadFromStart)
                            { 
                                _quadStripBuilder.Start(new float2x2(start.xy, start.zw), ref buffersData);
                                _quadStripBuilder.Continue(new float2x2(middle.xy, middle.zw), ref buffersData);
                            }
                            else if (fillData.ConstructType == QuadConstructType.NewQuadToEnd)
                            {
                                _quadStripBuilder.Start(new float2x2(middle.xy, middle.zw), ref buffersData);
                                _quadStripBuilder.Continue(new float2x2(end.xy, end.zw), ref buffersData);
                            }
                            else if (fillData.ConstructType == QuadConstructType.ContinueQuadFromStart)
                            {
                                Debug.Log($"local lerp param {localLerpParam}");
                                _quadStripBuilder.Continue(new float2x2(middle.xy, middle.zw), ref buffersData);
                            }
                            else
                            {
                                throw new System.ArgumentOutOfRangeException("Unknown QSTransSegment.ConstructType");
                            }
                        }
                    }
                }
            }

            Debug.Log("Updated transition mesh.");
        }
    }
}