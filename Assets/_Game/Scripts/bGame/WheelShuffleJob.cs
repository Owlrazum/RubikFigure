// using Unity.Burst;
// using Unity.Jobs;
// using Unity.Mathematics;
// using Unity.Collections;

// using UnityEngine;
// using UnityEngine.Rendering;
// using UnityEngine.Assertions;

// [BurstCompile]
// public struct WheelShuffleJob : IJobFor
// {
//     public int P_VertexCountInOneSegment;
//     public float P_ClockMoveBufferLerpValue; // Assert that it is less than 0.5f;

//     [ReadOnly]
//     public NativeArray<SegmentMoveType> InputSegmentMoveTypes;

//     [ReadOnly]
//     public NativeArray<float> InputLerpParams;

//     [ReadOnly]
//     public NativeArray<VertexData> InputVertices;

//     [ReadOnly]
//     public NativeArray<SegmentPoint> InputSegmentPoints;

//     [WriteOnly]
//     public NativeArray<VertexData> OutputVertices;

//     public void Execute(int segmentIndex)
//     {
//         int startVertexIndex = segmentIndex * P_VertexCountInOneSegment;
//         float lerpParam = InputLerpParams[segmentIndex];
//         SegmentPoint segmentPoint = InputSegmentPoints[segmentIndex];

//         SegmentMoveType segmentMove = InputSegmentMoveTypes[segmentIndex];
//         switch (segmentMove)
//         { 
//             case SegmentMoveType.Up:
//             case SegmentMoveType.Down:
//                 MoveSegmentUpDown(lerpParam, startVertexIndex, segmentPoint);
//                 break;
//             case SegmentMoveType.Clockwise:
//             case SegmentMoveType.CounterClockwise:
//                 MoveSegmentClockOrder(lerpParam, startVertexIndex, segmentPoint);
//                 break;
//         }
//     }

//     private void MoveSegmentUpDown(
//         float lerpParam, 
//         int startVertexIndex, 
//         SegmentPoint segmentPoint)
//     {
//         VertexData data;
//         for (int corner = 0; corner < 8; corner++)
//         {
//             int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner) + startVertexIndex;

//             data = InputVertices[meshCorner.x];
//             data.position = math.lerp(InputVertices[meshCorner.x].position,
//                 segmentPoint.GetCornerPosition(corner), lerpParam);
//             OutputVertices[meshCorner.x] = data;

//             data = InputVertices[meshCorner.y];
//             data.position = math.lerp(InputVertices[meshCorner.y].position,
//                 segmentPoint.GetCornerPosition(corner), lerpParam);
//             OutputVertices[meshCorner.y] = data;

//             data = InputVertices[meshCorner.z];
//             data.position = math.lerp(InputVertices[meshCorner.z].position,
//                 segmentPoint.GetCornerPosition(corner), lerpParam);
//             OutputVertices[meshCorner.z] = data;
//         }
//     }

//     private void MoveSegmentClockOrder(
//         float lerpParam, 
//         int startVertexIndex, 
//         SegmentPoint segmentPoint)
//     {
//         VertexData data;

//         if (lerpParam < P_ClockMoveBufferLerpValue)
//         {
//             lerpParam = math.unlerp(lerpParam, 0, 0.5f);
//             for (int corner = 0; corner < 4; corner++)
//             {
//                 #region ApproachSegmentPoint
//                 int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner) + startVertexIndex;

//                 data = InputVertices[meshCorner.x];
//                 data.position = math.lerp(InputVertices[meshCorner.x].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.x] = data;

//                 data = InputVertices[meshCorner.y];
//                 data.position = math.lerp(InputVertices[meshCorner.y].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.y] = data;

//                 data = InputVertices[meshCorner.z];
//                 data.position = math.lerp(InputVertices[meshCorner.z].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.z] = data;
//                 #endregion
//             }
//         }
//         else if (lerpParam < 0.5f)
//         {
//             float lerpParamOrigin = lerpParam;
//             lerpParam = math.unlerp(lerpParamOrigin, 0, 0.5f);

//             for (int corner = 0; corner < 4; corner++)
//             {
//                 #region ApproachSegmentPoint
//                 int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner) + startVertexIndex;

//                 data = InputVertices[meshCorner.x];
//                 data.position = math.lerp(InputVertices[meshCorner.x].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.x] = data;

//                 data = InputVertices[meshCorner.y];
//                 data.position = math.lerp(InputVertices[meshCorner.y].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.y] = data;

//                 data = InputVertices[meshCorner.z];
//                 data.position = math.lerp(InputVertices[meshCorner.z].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.z] = data;
//                 #endregion
//             }

//             lerpParam = math.unlerp(lerpParam, P_ClockMoveBufferLerpValue, 1);
//             for (int corner = 4; corner < 8; corner++)
//             {
//                 #region ApproachSegmentPoint
//                 int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner) + startVertexIndex;

//                 data = InputVertices[meshCorner.x];
//                 data.position = math.lerp(InputVertices[meshCorner.x].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.x] = data;

//                 data = InputVertices[meshCorner.y];
//                 data.position = math.lerp(InputVertices[meshCorner.y].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.y] = data;

//                 data = InputVertices[meshCorner.z];
//                 data.position = math.lerp(InputVertices[meshCorner.z].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.z] = data;
//                 #endregion
//             }
//         }
//         else
//         {
//             lerpParam = math.unlerp(lerpParam, P_ClockMoveBufferLerpValue, 1);
//             for (int corner = 4; corner < 8; corner++)
//             {
//                 #region ApproachSegmentPoint
//                 int3 meshCorner = WheelLookUpTable.GetCornerIndices(corner) + startVertexIndex;

//                 data = InputVertices[meshCorner.x];
//                 data.position = math.lerp(InputVertices[meshCorner.x].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.x] = data;

//                 data = InputVertices[meshCorner.y];
//                 data.position = math.lerp(InputVertices[meshCorner.y].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.y] = data;

//                 data = InputVertices[meshCorner.z];
//                 data.position = math.lerp(InputVertices[meshCorner.z].position,
//                     segmentPoint.GetCornerPosition(corner), lerpParam);
//                 OutputVertices[meshCorner.z] = data;
//                 #endregion
//             }
//         }
//     }
// }