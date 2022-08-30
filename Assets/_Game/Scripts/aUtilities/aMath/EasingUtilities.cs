using Unity.Mathematics;

namespace Orazum.Math
{ 
    public static class EasingUtilities
    {
        public static float EaseIn(float lerpParam)
        {
            return lerpParam * lerpParam;
        }

        public static float Flip(float t)
        {
            return 1 - t;
        }

        public static float EaseOut(float lerpParam)
        {
            return Flip(EaseIn(Flip(lerpParam)));
        }

        public static float EaseInOut(float lerpParam)
        {
            return math.lerp(EaseIn(lerpParam), EaseOut(lerpParam), lerpParam);
        }
    }
}
