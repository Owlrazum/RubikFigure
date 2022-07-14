using Unity.Mathematics;
using UnityEngine;

namespace Orazum.Utilities
{
    public enum ColorComponentType
    { 
        Red,
        Green,
        Blue
    }

    public static class ColorUtilities
    {
        public static float3 Color2HSL(Color color)
        {
            Color.RGBToHSV(color, out float H, out float S, out float V);
            return new float3(H, S, V);
        }

        public static float3 RGB2HSL(float3 rgb)
        {
            Color color = new Color(rgb.x, rgb.y, rgb.z);
            Color.RGBToHSV(color, out float H, out float S, out float V);
            return new float3(H, S, V);
        }
    }
}