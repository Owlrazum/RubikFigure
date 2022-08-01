// using System;
// using System.Collections;
// using System.Collections.Generic;
// using Unity.Mathematics;

// public struct WheelSegmentVertices : IEnumerable<VertexData>
// { 
//     private VertexData _bv1;
//     private VertexData _bv2;
//     private VertexData _bv3;
//     private VertexData _bv4;
//     private VertexData _bv5;
//     private VertexData _bv6;

//     private VertexData _tv1;
//     private VertexData _tv2;
//     private VertexData _tv3;
//     private VertexData _tv4;
//     private VertexData _tv5;
//     private VertexData _tv6;

//     public VertexData this[int index]
//     {
//         get
//         {
//             switch (index)
//             {
//                 case 0: return _bv1;
//                 case 1: return _bv2;
//                 case 2: return _bv3;
//                 case 3: return _bv4;
//                 case 4: return _bv5;
//                 case 5: return _bv6;
//                 case 6: return _tv1;
//                 case 7: return _tv2;
//                 case 8: return _tv3;
//                 case 9: return _tv4;
//                 case 10: return _tv5;
//                 case 11: return _tv6;
//                 default: throw new ArgumentOutOfRangeException($"There are only 12 vertices! You tried to access the vertex at index {index}");
//             }
//         }
//         set
//         {
//             switch (index)
//             {
//                 case 0:
//                     _bv1 = value;
//                     break;
//                 case 1:
//                     _bv2 = value;
//                     break;
//                 case 2:
//                     _bv3 = value;
//                     break;
//                 case 3:
//                     _bv4 = value;
//                     break;
//                 case 4:
//                     _bv5 = value;
//                     break;
//                 case 5:
//                     _bv6 = value;
//                     break;
//                 case 6: 
//                     _tv1 = value;
//                     break;
//                 case 7: 
//                     _tv2 = value;
//                     break;
//                 case 8: 
//                     _tv3 = value;
//                     break;
//                 case 9: 
//                     _tv4 = value;
//                     break;
//                 case 10: 
//                     _tv5 = value;
//                     break;
//                 case 11: 
//                     _tv6 = value;
//                     break;
//                 default: throw new ArgumentOutOfRangeException($"There are only 12 vertices! You tried to access the vertex at index {index}");
//             }
//         }
//     }

//     public IEnumerator<VertexData> GetEnumerator()
//     {
//         for (int i = 0; i < 12; i++)
//         {
//             yield return this[i];
//         }
//     }

//     IEnumerator IEnumerable.GetEnumerator()
//     {
//         return GetEnumerator();
//     }
// }
