using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

using static Orazum.Math.LineSegmentUtilities;
using static QST_Segment;
using static QSTS_FillData;

// TODO: Write tests?
namespace Orazum.Meshing
{
    public struct QSTS_QuadBuilder
    {
        public void BuildFadeOutTransition(
            QuadStrip qs,
            ref NativeArray<QST_Segment> writeBuffer
        )
        {
            int builderIndexer = 0;

            float stripLength = qs.ComputeSingleLength();
            float2 lerpOffsets = float2.zero;
            writeBuffer[builderIndexer++] = BuildFirstFadeOutTransSeg(in qs, stripLength, ref lerpOffsets);
            for (int i = 1; i < qs.QuadsCount; i++)
            {
                writeBuffer[builderIndexer++] = BuildUsualFadeOutTransSeg(qs, i, stripLength, ref lerpOffsets);
            }
        }

        public void BuildFadeInTransition(
            QuadStrip qs,
            ref NativeArray<QST_Segment> writeBuffer
        )
        {
            int builderIndexer = 0;

            float stripLength = qs.ComputeSingleLength();
            float2 lerpOffsets = float2.zero;
            for (int i = 0; i < qs.QuadsCount - 1; i++)
            {
                writeBuffer[builderIndexer++] = BuildUsualFadeInTransSeg(qs, i, stripLength, ref lerpOffsets);
            }
            writeBuffer[builderIndexer++] = BuildLastFadeInTransSeg(qs, qs.QuadsCount - 1, stripLength, ref lerpOffsets);
        }

        #region FadeOut
        private QST_Segment BuildFirstFadeOutTransSeg(in QuadStrip qs, float stripLength, ref float2 lerpOffsets)
        {
            QST_Segment firstFadeOut = new QST_Segment(qs[0], qs[1], 1);
            firstFadeOut.Type = QSTS_Type.Quad;

            float lengthRatio = DistanceLineSegment(qs[0][0], qs[1][0]) / stripLength;
            MoveLerpOffsets(ref lerpOffsets, lengthRatio);

            firstFadeOut[0] = FillOut(lerpOffsets);

            return firstFadeOut;
        }

        private QST_Segment BuildUsualFadeOutTransSeg(in QuadStrip qs, int index, float stripLength, ref float2 lerpOffsets)
        {
            QST_Segment fadeOutSeg = new QST_Segment(qs[index], qs[index + 1], 2);
            fadeOutSeg.Type = QSTS_Type.Quad;

            float lengthRatio = DistanceLineSegment(qs[index][0], qs[index + 1][0]) / stripLength;
            MoveLerpOffsets(ref lerpOffsets, lengthRatio);

            fadeOutSeg[0] = Filled(new float2(0, lerpOffsets.x), isNewQuad: false);
            fadeOutSeg[1] = FillOut(lerpOffsets);

            return fadeOutSeg;
        }

        #endregion
        #region FadeIn
        private QST_Segment BuildUsualFadeInTransSeg(in QuadStrip qs, int index, float stripLength, ref float2 lerpOffsets)
        {
            QST_Segment fadeInSeg = new QST_Segment(qs[index], qs[index + 1], 2);
            fadeInSeg.Type = QSTS_Type.Quad;

            float lengthRatio = DistanceLineSegment(qs[index][0], qs[index + 1][0]) / stripLength;
            MoveLerpOffsets(ref lerpOffsets, lengthRatio);

            fadeInSeg[0] = FillIn(lerpOffsets, isNewQuad: index == 0);
            fadeInSeg[1] = Filled(new float2(lerpOffsets.y, 1), isNewQuad: index == 0);

            return fadeInSeg;
        }

        private QST_Segment BuildLastFadeInTransSeg(in QuadStrip qs, int index, float stripLength, ref float2 lerpOffsets)
        {
            QST_Segment lastFadeIn = new QST_Segment(qs[index], qs[index + 1], 1);
            lastFadeIn.Type = QSTS_Type.Quad;

            float lengthRatio = DistanceLineSegment(qs[index][0], qs[index + 1][0]) / stripLength;
            MoveLerpOffsets(ref lerpOffsets, lengthRatio);
            lerpOffsets.y = 1;

            lastFadeIn[0] = FillIn(lerpOffsets, isNewQuad: false);

            return lastFadeIn;
        }
        #endregion
        private QSTS_FillData FillOut(in float2 lerpRange)
        {
            QSTS_FillData fillOut = new QSTS_FillData(ConstructType.New, FillType.ToEnd, lerpRange);
            return fillOut;
        }

        private QSTS_FillData FillIn(in float2 lerpRange, bool isNewQuad)
        {
            ConstructType constructType = isNewQuad ? ConstructType.New : ConstructType.Continue;
            QSTS_FillData fillIn = new QSTS_FillData(constructType, FillType.FromStart, lerpRange);
            return fillIn;
        }

        private QSTS_FillData Filled(in float2 lerpRange, bool isNewQuad)
        {
            ConstructType constructType = isNewQuad ? ConstructType.New : ConstructType.Continue;
            QSTS_FillData filledData = new QSTS_FillData(constructType, FillType.StartToEnd, lerpRange);
            return filledData;
        }

        private void MoveLerpOffsets(ref float2 lerpOffsets, float value)
        {
            lerpOffsets.x = lerpOffsets.y;
            lerpOffsets.y += value;
        }
    }
}