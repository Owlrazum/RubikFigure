using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;

namespace Orazum.Meshing
{
    public enum LerpRangeFillType
    { 
        StartFill,
        UsualFill
    }


    public struct QuadStripTransition
    {
        public static float FillTypeToFloat(LerpRangeFillType lerpRangeFillType)
        { 
            switch (lerpRangeFillType)
            {
                case LerpRangeFillType.UsualFill:
                    return 0;
                case LerpRangeFillType.StartFill:
                    return 10;
            }

            throw new System.ArgumentOutOfRangeException("Unknown type of LerpRangeFillType");
        }

        public static LerpRangeFillType FloatToFillType(float value)
        {
            if (value < 5)
            {
                return LerpRangeFillType.UsualFill;
            }
            else 
            {
                return LerpRangeFillType.StartFill;
            }
        }


        private NativeArray<float4x2>.ReadOnly _transitionPositions;
        private NativeArray<float3>.ReadOnly _lerpRanges;

        private QuadStripBuilderVertexData _quadStripBuilder;

        /// <summary>
        /// transitionPositions contain data about quadStripSegments, 
        /// with associated lerpRanges on one to one relationship.
        /// the z component of lerpRanges element contain data about whether this range is closed ended, 
        /// and next lerpRange should start quadStrip.
        /// </summary>
        public QuadStripTransition(
            ref NativeArray<VertexData> vertices,
            ref NativeArray<short> indices
        )
        { 
            _transitionPositions = new NativeArray<float4x2>.ReadOnly();
            _lerpRanges = new NativeArray<float3>.ReadOnly();

            _quadStripBuilder = new QuadStripBuilderVertexData(vertices, indices);
        }

        public void AssignTransitionData(
            NativeArray<float4x2>.ReadOnly transitionPositions,
            NativeArray<float3>.ReadOnly lerpRanges
        )
        { 
            _transitionPositions = transitionPositions;
            _lerpRanges = lerpRanges;
        }

        public void UpdateWithLerpPos(float globalLerpParam, ref MeshBuffersData buffersData)
        {
            float localLerpParam = 0;
            for (int i = 0; i < _transitionPositions.Length; i++)
            {
                float4 start = _transitionPositions[i][0];
                float4 end   = _transitionPositions[i][1];
                float2 lerpRange = _lerpRanges[i].xy;
                LerpRangeFillType fillType = FloatToFillType(_lerpRanges[i].z);
                if (globalLerpParam >= lerpRange.x)
                {
                    if (fillType == LerpRangeFillType.StartFill)
                    { 
                        Debug.Log($"Start {_transitionPositions.Length} pos: {start.xy} {start.zw};buffersData: {buffersData.ToString()}");
                        _quadStripBuilder.Start(new float2x2(start.xy, start.zw), ref buffersData);
                    }
                    if (globalLerpParam >= lerpRange.y)
                    { 
                        Debug.Log($"ContinueFill {_transitionPositions.Length} pos: {end.xy} {end.zw};buffersData: {buffersData.ToString()}");
                        _quadStripBuilder.Continue(new float2x2(end.xy, end.zw), ref buffersData);
                    }
                    else
                    { 
                        localLerpParam = math.unlerp(lerpRange.x, lerpRange.y, 
                            globalLerpParam - lerpRange.x);
                        float4 middle = math.lerp(start, end, localLerpParam);
                        Debug.Log($"ContinuePart {_transitionPositions.Length} pos: {middle.xy} {middle.zw};buffersData: {buffersData.ToString()}");
                        _quadStripBuilder.Continue(new float2x2(middle.xy, middle.zw), ref buffersData);
                    }
                }
            }

            Debug.Log("Updated transition mesh.");
        }
    }
}