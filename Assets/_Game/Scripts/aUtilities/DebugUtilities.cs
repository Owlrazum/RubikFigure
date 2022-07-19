using UnityEngine;

namespace Orazum.Utilities
{ 
    public static class DebugUtilities
    {
        public static void DrawGridCell(Vector3 pos, float rayLength, float time = -1)
        {
            Vector3 halfForward = rayLength / 2 * Vector3.forward;
            Vector3 halfRight = rayLength / 2 * Vector3.right;

            if (time < 0)
            { 
                Debug.DrawRay(pos - halfForward, rayLength * Vector3.forward, Color.red);
                Debug.DrawRay(pos - halfRight, rayLength * Vector3.right, Color.red);
            }
            else
            { 
                Debug.DrawRay(pos - halfForward, rayLength * Vector3.forward, Color.red, time);
                Debug.DrawRay(pos - halfRight, rayLength * Vector3.right, Color.red, time);
            }
        }
    }
}