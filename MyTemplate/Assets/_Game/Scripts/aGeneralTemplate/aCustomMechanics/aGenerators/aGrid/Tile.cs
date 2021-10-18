using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Generators
{
    /// <summary>
    /// Used for triangular grids. Checks if triangle points "up" or "down"
    /// </summary>
    public enum OrientType
    {
        Up,
        Down
    }

    public class Tile : MonoBehaviour
    {
        public void AssignOrient(OrientType orient)
        {
            // init orient here if you need it;
        }

        public void AssignIndex(int index)
        {
            // can use index in an array as ID;
        }
    }
}
