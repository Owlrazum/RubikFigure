using Unity.Burst;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Collections;

using UnityEngine;
using UnityEngine.Assertions;

[BurstCompile]
public struct WheelGenJob : IJob
{
    public float P_WheelHeight;
    public float P_OuterCircleRadius;
    public float P_InnerCircleRadius;
    public int P_SideCount;
    public int P_SegmentsCountInOneSide;

    // The size of array is an amount of segments
    public NativeArray<short> OutputVertexCounts;

    // The size of array is an amount of segments
    public NativeArray<short> OutputIndexCounts;

    [WriteOnly]
    public NativeArray<VertexData> OutputVertices;

    [WriteOnly]
    public NativeArray<short> OutputIndices;

    [WriteOnly]
    public NativeArray<SegmentPoint> OutputSegmentPoints;

    private short totalVertexCount;
    private short totalIndexCount;

    private float2 _uv;
    private int _segmentIndex;
    private float _currentRadius;
    private float _nextRadius;

    // private bool DoesMultiVertexSelectionInOneSegmentNeedInitialization;

    public void Execute()
    {
        // DoesMultiVertexSelectionInOneSegmentNeedInitialization = true;
        Assert.IsTrue(P_SideCount >= 3 && P_SideCount <= 6);

        CircleRaysStruct rays = new CircleRaysStruct();
#region RaysInit
        float angleDelta = 2 * math.PI / P_SideCount;
        float currentAngle = math.PI / 2 - angleDelta ;
        rays[0] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
        currentAngle += angleDelta;
        rays[1] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
        currentAngle += angleDelta;
        rays[2] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
        currentAngle += angleDelta;

        if (P_SideCount >= 4)
        { 
            rays[3] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
            currentAngle += angleDelta;
        }
        if (P_SideCount >= 5)
        { 
            rays[4] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
            currentAngle += angleDelta;
        }
        if (P_SideCount >= 6)
        { 
            rays[5] = new float3(math.cos(currentAngle), 0, math.sin(currentAngle));
            currentAngle += angleDelta;
        }
#endregion

        
        float radiusDelta = (P_OuterCircleRadius - P_InnerCircleRadius) / P_SegmentsCountInOneSide;

        for (int i = 0; i < P_SideCount; i++)
        {
            int currentRayIndex = i;
            int nextRayIndex = i + 1 >= P_SideCount ? 0 : i + 1;

            _uv = new float2(0, i * 1.0f / 6);

            _currentRadius = P_InnerCircleRadius;
            _nextRadius = _currentRadius + radiusDelta;
            
            for (int j = 0; j < P_SegmentsCountInOneSide; j++)
            {
                _segmentIndex = i * P_SegmentsCountInOneSide + j;

                // if (DoesMultiVertexSelectionInOneSegmentNeedInitialization)
                // {
                //     AddSegmentAndInitVertexIndices(rays[currentRayIndex], rays[nextRayIndex]);
                //     DoesMultiVertexSelectionInOneSegmentNeedInitialization = false;
                // }
                // else
                // { 
                    OutputSegmentPoints[_segmentIndex] = AddSegment(rays[currentRayIndex], rays[nextRayIndex]);
                    
                // }

                _currentRadius = _nextRadius;
                _nextRadius += radiusDelta;
            }
        }

        // Debug.Log(
        //     "BBL " + BBL + "\n" +
        //     "BBR " + BBR + "\n" +
        //     "BTL " + BTL + "\n" +
        //     "BTR " + BTR + "\n" +
        //     "FBL " + FBL + "\n" +
        //     "FBR " + FBR + "\n" +
        //     "FTL " + FTL + "\n" +
        //     "FTR " + FTR
        // );
    }

    private SegmentPoint AddSegment(float3 currentRay, float3 nextRay)
    {
        OutputVertexCounts[_segmentIndex] = 0;
        OutputIndexCounts[_segmentIndex] = 0;

        float3x4 posBot = new float3x4(
            currentRay * _currentRadius,
            currentRay * _nextRadius,
            nextRay * _currentRadius,
            nextRay * _nextRadius
        );

        float3x4 posQuadBot = new float3x4(
            posBot[2],
            posBot[3],
            posBot[1],
            posBot[0]
        );

        // 0123 
        // 2310 3201
        // 1023 2301
        
        AddQuad(posQuadBot, math.down());
        
        float3x4 posTop = new float3x4(
            posBot[0] + math.up() * P_WheelHeight,
            posBot[1] + math.up() * P_WheelHeight,
            posBot[2] + math.up() * P_WheelHeight,
            posBot[3] + math.up() * P_WheelHeight
        );

        float3x4 posQuadTop = new float3x4(
            posTop[0],
            posTop[1],
            posTop[3],
            posTop[2]
        );

        // 4567
        // 0132 4576
        // 1023 5476
        AddQuad(posQuadTop, math.up());

        float3x4 posLeft = new float3x4(
            posBot[1],
            posTop[1],
            posTop[0],
            posBot[0]
        );
        float3 left = math.rotate(quaternion.AxisAngle(math.up(), -90), currentRay);;
        AddQuad(posLeft, left);

        float3x4 posForward = new float3x4(
            posBot[3],
            posTop[3],
            posTop[1],
            posBot[1]
        );
        float3 forward = (currentRay + nextRay) / 2;
        AddQuad(posForward, forward);

        float3x4 posRight = new float3x4(
            posBot[2],
            posTop[2],
            posTop[3],
            posBot[3]
        );
        float3 right = math.rotate(quaternion.AxisAngle(math.up(), 90), nextRay);;
        AddQuad(posRight, right);

        float3x4 posBack = new float3x4(
            posBot[0],
            posTop[0],
            posTop[2],
            posBot[2]
        );
        float3 back = -forward;
        AddQuad(posBack, back);

        SegmentPoint segmentPoint = new SegmentPoint
        {
            BBL = posBot[0],
            BTL = posTop[0],
            FBL = posBot[1],
            FTL = posTop[1],

            BBR = posBot[2],
            BTR = posTop[2],
            FBR = posBot[3],
            FTR = posTop[3]
        };

        return segmentPoint;
    }

    private void AddQuad(float3x4 positions, float3 normal)
    { 
        AddVertex(positions[1], normal);
        short diagonal_1 = AddVertex(positions[0], normal);
        short diagonal_2 = AddVertex(positions[2], normal);

        AddIndex(diagonal_2);
        AddIndex(diagonal_1);
        AddVertex(positions[3], normal);
    }

    private short AddVertex(float3 pos, float3 normal)
    { 
        VertexData vertex = new VertexData();
        vertex.position = pos;
        vertex.normal = normal;
        vertex.uv = _uv;
        
        OutputVertices[totalVertexCount++] = vertex;
        short addedVertexIndex = OutputVertexCounts[_segmentIndex];
        OutputVertexCounts[_segmentIndex]++;

        OutputIndices[totalIndexCount++] = addedVertexIndex;
        OutputIndexCounts[_segmentIndex]++;

        return addedVertexIndex;
    }

    private void AddIndex(short vertexIndex)
    {
        OutputIndexCounts[_segmentIndex]++;
        OutputIndices[totalIndexCount++] = vertexIndex;
    }

    // private enum SegmentFace
    // { 
    //     Bottom,
    //     Top,
    //     Left,
    //     Forward,
    //     Right,
    //     Back
    // }

    // public int3 BBL;
    // public int3 BBR;
    // public int3 BTL;
    // public int3 BTR;
    
    // public int3 FBL;
    // public int3 FBR;
    // public int3 FTL;
    // public int3 FTR;

    // private void AddSegmentAndInitVertexIndices(float3 currentRay, float3 nextRay)
    // {
    //     BBL = int3.zero;
    //     BBR = int3.zero;
    //     BTL = int3.zero;
    //     BTR = int3.zero;

    //     FBL = int3.zero;
    //     FBR = int3.zero;
    //     FTL = int3.zero;
    //     FTR = int3.zero;

    //     OutputVertexCounts[_segmentIndex] = 0;
    //     OutputIndexCounts[_segmentIndex] = 0;

    //     float3x4 posBot = new float3x4(
    //         currentRay * _currentRadius, 
    //         currentRay * _nextRadius,
    //         nextRay * _currentRadius,
    //         nextRay * _nextRadius
    //     );

    //     float3x4 posQuadBot = new float3x4(
    //         posBot[2],
    //         posBot[3],
    //         posBot[1],
    //         posBot[0]
    //     );

    //     AddQuadAndInitVertexIndices(posQuadBot, math.down(), SegmentFace.Bottom);
        
    //     float3x4 posTop = new float3x4(
    //         posBot[0] + math.up() * P_WheelHeight,
    //         posBot[1] + math.up() * P_WheelHeight,
    //         posBot[2] + math.up() * P_WheelHeight,
    //         posBot[3] + math.up() * P_WheelHeight
    //     );

    //     float3x4 posQuadTop = new float3x4(
    //         posTop[0],
    //         posTop[1],
    //         posTop[3],
    //         posTop[2]
    //     );

    //     // 4567
    //     // 0132 4576
    //     // 1023 5476
    //     AddQuadAndInitVertexIndices(posQuadTop, math.up(), SegmentFace.Top);

    //     float3x4 posLeft = new float3x4(
    //         posBot[1],
    //         posTop[1],
    //         posTop[0],
    //         posBot[0]
    //     );
    //     float3 left = math.rotate(quaternion.AxisAngle(math.up(), -90), currentRay);;
    //     AddQuadAndInitVertexIndices(posLeft, left, SegmentFace.Left);

    //     float3x4 posForward = new float3x4(
    //         posBot[3],
    //         posTop[3],
    //         posTop[1],
    //         posBot[1]
    //     );
    //     float3 forward = (currentRay + nextRay) / 2;
    //     AddQuadAndInitVertexIndices(posForward, forward, SegmentFace.Forward);

    //     float3x4 posRight = new float3x4(
    //         posBot[2],
    //         posTop[2],
    //         posTop[3],
    //         posBot[3]
    //     );
    //     float3 right = math.rotate(quaternion.AxisAngle(math.up(), 90), nextRay);;
    //     AddQuadAndInitVertexIndices(posRight, right, SegmentFace.Right);

    //     float3x4 posBack = new float3x4(
    //         posBot[0],
    //         posTop[0],
    //         posTop[2],
    //         posBot[2]
    //     );
    //     float3 back = -forward;
    //     AddQuadAndInitVertexIndices(posBack, back, SegmentFace.Back);
    // }

    // private void AddQuadAndInitVertexIndices(float3x4 positions, float3 normal, SegmentFace segmentFace)
    // {
    //     FirstInitSegmentVertexIndex(segmentFace, OutputVertexCounts[0]);
    //     AddVertex(positions[1], normal); 
    //     SecondInitSegmentVertexIndex(segmentFace, OutputVertexCounts[0]);
    //     short diagonal_1 = AddVertex(positions[0], normal);
    //     ThirdInitSegmentVertexIndex(segmentFace, OutputVertexCounts[0]);
    //     short diagonal_2 = AddVertex(positions[2], normal);

    //     AddIndex(diagonal_2);
    //     AddIndex(diagonal_1);
    //     FourthInitSegmentVertexIndex(segmentFace, OutputVertexCounts[0]);
    //     AddVertex(positions[3], normal);
    // }

    // private void FirstInitSegmentVertexIndex(SegmentFace segmentFace, int value)
    // {
    //     switch (segmentFace)
    //     { 
    //         case SegmentFace.Bottom:
    //             FBR.x = value;
    //             break;
    //         case SegmentFace.Top:
    //             FTL.x = value;
    //             break;
    //         case SegmentFace.Left:
    //             FTL.y = value;
    //             break;
    //         case SegmentFace.Forward:
    //             FTR.y = value;
    //             break;
    //         case SegmentFace.Right:
    //             BTR.y = value;
    //             break;
    //         case SegmentFace.Back:
    //             BTL.z = value;
    //             break;
    //     }
    // }

    // private void SecondInitSegmentVertexIndex(SegmentFace segmentFace, int value)
    // { 
    //     switch (segmentFace)
    //     { 
    //         case SegmentFace.Bottom:
    //             BBR.x = value;
    //             break;
    //         case SegmentFace.Top:
    //             BTL.x = value;
    //             break;
    //         case SegmentFace.Left:
    //             FBL.y = value;
    //             break;
    //         case SegmentFace.Forward:
    //             FBR.y = value;
    //             break;
    //         case SegmentFace.Right:
    //             BBR.y = value;
    //             break;
    //         case SegmentFace.Back:
    //             BBL.z = value;
    //             break;
    //     }
    // }

    // private void ThirdInitSegmentVertexIndex(SegmentFace segmentFace, int value)
    // { 
    //     switch (segmentFace)
    //     { 
    //         case SegmentFace.Bottom:
    //             FBL.x = value;
    //             break;
    //         case SegmentFace.Top:
    //             FTR.x = value;
    //             break;
    //         case SegmentFace.Left:
    //             BTL.y = value;
    //             break;
    //         case SegmentFace.Forward:
    //             FTL.z = value;
    //             break;
    //         case SegmentFace.Right:
    //             FTR.z = value;
    //             break;
    //         case SegmentFace.Back:
    //             BTR.z = value;
    //             break;
    //     }
    // }

    // private void FourthInitSegmentVertexIndex(SegmentFace segmentFace, int value)
    // { 
    //     switch (segmentFace)
    //     { 
    //         case SegmentFace.Bottom:
    //             BBL.x = value;
    //             break;
    //         case SegmentFace.Top:
    //             BTR.x = value;
    //             break;
    //         case SegmentFace.Left:
    //             BBL.y = value;
    //             break;
    //         case SegmentFace.Forward:
    //             FBL.z = value;
    //             break;
    //         case SegmentFace.Right:
    //             FBR.z = value;
    //             break;
    //         case SegmentFace.Back:
    //             BBR.z = value;
    //             break;
    //     }
    // }
}