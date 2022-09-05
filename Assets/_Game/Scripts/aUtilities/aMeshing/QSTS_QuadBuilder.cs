using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using static Orazum.Math.LineSegmentUtilities;
using static QST_Segment;
using static QSTS_FillData;

namespace Orazum.Meshing
{
    public struct QSTS_QuadBuilder
    {
        public void BuildFadeOutTransition(
            QuadStrip qs,
            ref NativeArray<QST_Segment> writeBuffer
        )
        {
            Assert.IsTrue(writeBuffer.Length <= qs.QuadsCount);
            int builderIndexer = 0;

            float stripLength = qs.ComputeSingleLength();
            writeBuffer[builderIndexer++] = BuildFirstFadeOutTransSeg(in qs, stripLength, out float2 lerpOffsets); ;
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
            Assert.IsTrue(writeBuffer.Length <= qs.QuadsCount);
            int builderIndexer = 0;

            float stripLength = qs.ComputeSingleLength();
            float2 lerpOffsets = float2.zero;
            for (int i = 0; i < qs.QuadsCount - 1; i++)
            {
                writeBuffer[builderIndexer++] = BuildUsualFadeInTransSeg(qs, i, stripLength, ref lerpOffsets);
            }
            writeBuffer[builderIndexer++] = BuildLastFadeInTransSeg(qs, qs.QuadsCount, stripLength, ref lerpOffsets);
        }

        #region FadeOut
        private QST_Segment BuildFirstFadeOutTransSeg(in QuadStrip qs, float stripLength, out float2 lerpOffsets)
        {
            QST_Segment firstFadeOut = new QST_Segment(qs[0], qs[1], 1);
            firstFadeOut.Type = QSTS_Type.Quad;
            float quadLength = DistanceLineSegment(qs[0][0], qs[1][0]);
            lerpOffsets = float2.zero;
            firstFadeOut[0] = BuildFillOut(quadLength / stripLength, ref lerpOffsets); ;
            return firstFadeOut;
        }

        private QST_Segment BuildUsualFadeOutTransSeg(in QuadStrip qs, int index, float stripLength, ref float2 lerpOffsets)
        {
            QST_Segment fadeOutSeg = new QST_Segment(qs[index], qs[index + 1], 2);
            fadeOutSeg.Type = QSTS_Type.Quad;
            float quadLength = DistanceLineSegment(qs[index][0], qs[index + 1][0]);
            float lengthRatio = quadLength / stripLength;
            fadeOutSeg[0] = BuildFilledWhileFadeOut(lengthRatio, ref lerpOffsets);
            fadeOutSeg[1] = BuildFillOut(lengthRatio, ref lerpOffsets);
            return fadeOutSeg;
        }

        private QSTS_FillData BuildFilledWhileFadeOut(float lengthRatio, ref float2 lerpOffset)
        {
            lerpOffset.y += lengthRatio;
            float2 lerpRange = new float2(0, lerpOffset.y);
            QSTS_FillData filledData = new QSTS_FillData(FillType.ContinueStartToEnd, lerpRange);
            return filledData;
        }

        private QSTS_FillData BuildFillOut(float lengthRatio, ref float2 lerpOffset)
        {
            float2 lerpRange = new float2(lerpOffset.x, lerpOffset.x + lengthRatio);
            lerpOffset.x = lerpRange.y;
            QSTS_FillData fillOut = new QSTS_FillData(FillType.NewToEnd, lerpRange);
            return fillOut;
        }
        #endregion
        #region FadeIn
        private QST_Segment BuildLastFadeInTransSeg(in QuadStrip qs, int index, float stripLength, ref float2 lerpOffsets)
        {
            QST_Segment lastFadeIn = new QST_Segment(qs[index], qs[index + 1], 1);
            lastFadeIn.Type = QSTS_Type.Quad;
            float quadLength = DistanceLineSegment(qs[index][0], qs[index + 1][0]);
            lastFadeIn[0] = BuildFillIn(quadLength / stripLength, ref lerpOffsets); ;
            return lastFadeIn;
        }

        private QST_Segment BuildUsualFadeInTransSeg(in QuadStrip qs, int index, float stripLength, ref float2 lerpOffsets)
        {
            QST_Segment FadeInSeg = new QST_Segment(qs[index], qs[index + 1], 2);
            FadeInSeg.Type = QSTS_Type.Quad;
            float quadLength = DistanceLineSegment(qs[index][0], qs[index + 1][0]);
            float lengthRatio = quadLength / stripLength;
            FadeInSeg[0] = BuildFilledWhileFadeIn(lengthRatio, ref lerpOffsets, index == 0);
            FadeInSeg[1] = BuildFillIn(lengthRatio, ref lerpOffsets);
            return FadeInSeg;
        }

        private QSTS_FillData BuildFilledWhileFadeIn(float lengthRatio, ref float2 lerpOffset, bool isFirst)
        {
            lerpOffset.y += lengthRatio;
            float2 lerpRange = new float2(lerpOffset.y, 1);
            FillType fillType = isFirst ? FillType.NewStartToEnd : FillType.ContinueStartToEnd;
            QSTS_FillData filledData = new QSTS_FillData(fillType, lerpRange);
            return filledData;
        }

        private QSTS_FillData BuildFillIn(float lengthRatio, ref float2 lerpOffset)
        {
            float2 lerpRange = new float2(lerpOffset.x, lerpOffset.x + lengthRatio);
            lerpOffset.x = lerpRange.y;
            QSTS_FillData fillIn = new QSTS_FillData(FillType.NewFromStart, lerpRange);
            return fillIn;
        }
        #endregion
    }
}