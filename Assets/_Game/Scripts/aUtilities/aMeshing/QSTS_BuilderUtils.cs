using Unity.Mathematics;
using Unity.Collections;

using UnityEngine.Assertions;

using Orazum.Math;
using static Orazum.Math.LineSegmentUtilities;
using static Orazum.Meshing.QST_Segment;
using static Orazum.Meshing.QSTSFD_Radial;
using static Orazum.Meshing.QSTS_FillData;

namespace Orazum.Meshing
{ 
    public static class QSTS_BuilderUtils
    {
        public static void PrepareSegment(
            in QuadStrip qs, 
            QSTS_Type type, 
            int fillDataLength, 
            out QST_Segment qsts
        )
        {
            float3x2 start = qs[0];
            float3x2 end = qs[qs.LineSegmentsCount - 1];

            qsts = new QST_Segment(type, start, end, fillDataLength);
        }

        public static void PrepareSegment(
            in float3x2 start, 
            in float3x2 end, 
            QSTS_Type type, 
            int fillDataLength, 
            out QST_Segment qsts
        )
        { 
            qsts = new QST_Segment(type, start, end, fillDataLength);
        }
    }
}
