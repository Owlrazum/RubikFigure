using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using static Orazum.Math.LineSegmentUtilities;
using static QSTransSegment;

namespace Orazum.Meshing
{
    public struct QSTransitionBuilder
    {
        public void BuildFadeOutTransition(
            QuadStrip qs,
            ref NativeArray<QSTransSegment> writeBuffer
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
            ref NativeArray<QSTransSegment> writeBuffer
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
        private QSTransSegment BuildFirstFadeOutTransSeg(in QuadStrip qs, float stripLength, out float2 lerpOffsets)
        {
            QSTransSegment firstFadeOut = new QSTransSegment(qs[0], qs[1], 1);
            float quadLength = DistanceLineSegment(qs[0][0], qs[1][0]);
            lerpOffsets = float2.zero;
            firstFadeOut[0] = BuildFillOut(quadLength / stripLength, ref lerpOffsets); ;
            return firstFadeOut;
        }

        private QSTransSegment BuildUsualFadeOutTransSeg(in QuadStrip qs, int index, float stripLength, ref float2 lerpOffsets)
        {
            QSTransSegment fadeOutSeg = new QSTransSegment(qs[index], qs[index + 1], 2);
            float quadLength = DistanceLineSegment(qs[index][0], qs[index + 1][0]);
            float lengthRatio = quadLength / stripLength;
            fadeOutSeg[0] = BuildFilledWhileFadeOut(lengthRatio, ref lerpOffsets);
            fadeOutSeg[1] = BuildFillOut(lengthRatio, ref lerpOffsets);
            return fadeOutSeg;
        }

        private QSTransSegFillData BuildFilledWhileFadeOut(float lengthRatio, ref float2 lerpOffset)
        {
            lerpOffset.y += lengthRatio;
            float2 lerpRange = new float2(0, lerpOffset.y);
            QSTransSegFillData filledData = new QSTransSegFillData(lerpRange, MeshConstructType.Quad);
            filledData.QuadType = QuadConstructType.ContinueQuadStartToEnd;
            return filledData;
        }

        private QSTransSegFillData BuildFillOut(float lengthRatio, ref float2 lerpOffset)
        {
            float2 lerpRange = new float2(lerpOffset.x, lerpOffset.x + lengthRatio);
            lerpOffset.x = lerpRange.y;
            QSTransSegFillData fillOut = new QSTransSegFillData(lerpRange, MeshConstructType.Quad);
            fillOut.QuadType = QuadConstructType.NewQuadToEnd;
            return fillOut;
        }
        #endregion
        #region FadeIn
        private QSTransSegment BuildLastFadeInTransSeg(in QuadStrip qs, int index, float stripLength, ref float2 lerpOffsets)
        {
            QSTransSegment lastFadeIn = new QSTransSegment(qs[index], qs[index + 1], 1);
            float quadLength = DistanceLineSegment(qs[index][0], qs[index + 1][0]);
            lastFadeIn[0] = BuildFillIn(quadLength / stripLength, ref lerpOffsets); ;
            return lastFadeIn;
        }

        private QSTransSegment BuildUsualFadeInTransSeg(in QuadStrip qs, int index, float stripLength, ref float2 lerpOffsets)
        {
            QSTransSegment FadeInSeg = new QSTransSegment(qs[index], qs[index + 1], 2);
            float quadLength = DistanceLineSegment(qs[index][0], qs[index + 1][0]);
            float lengthRatio = quadLength / stripLength;
            FadeInSeg[0] = BuildFilledWhileFadeIn(lengthRatio, ref lerpOffsets, index == 0);
            FadeInSeg[1] = BuildFillIn(lengthRatio, ref lerpOffsets);
            return FadeInSeg;
        }

        private QSTransSegFillData BuildFilledWhileFadeIn(float lengthRatio, ref float2 lerpOffset, bool isFirst)
        {
            lerpOffset.y += lengthRatio;
            float2 lerpRange = new float2(lerpOffset.y, 1);
            QSTransSegFillData filledData = new QSTransSegFillData(lerpRange, MeshConstructType.Quad);
            filledData.QuadType = isFirst ? QuadConstructType.NewQuadStartToEnd : QuadConstructType.ContinueQuadStartToEnd;
            return filledData;
        }

        private QSTransSegFillData BuildFillIn(float lengthRatio, ref float2 lerpOffset)
        {
            float2 lerpRange = new float2(lerpOffset.x, lerpOffset.x + lengthRatio);
            lerpOffset.x = lerpRange.y;
            QSTransSegFillData fillIn = new QSTransSegFillData(lerpRange, MeshConstructType.Quad);
            fillIn.QuadType = QuadConstructType.NewQuadFromStart;
            return fillIn;
        }
        #endregion
    }
}