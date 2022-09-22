using Unity.Mathematics;
using Orazum.Math;

namespace Orazum.Meshing
{ 
    public struct QSTSFD_Radial
    {
        // RL: rotationLerp 
        // NQ: new quad
        // CQ: continue quad
        public enum RadialType
        {
            FirstOrderRotation, // Single rotateOnce, one quadStrip
            SecondOrderRotation, // Double rotateTwice, one quadStrip, donut shape.
            Move
        }
        public RadialType Type { get; private set; }

        // negative lerpLength signifies invalid state
        public QSTSFD_Radial(float invalidUselessParameter)
        {
            MaxLerpLength = -1;

            Type = RadialType.FirstOrderRotation;
            PrimaryAxisAngle = float4.zero;
            SecondOrderAngle = 0;
            RotationCenter = float3.zero;
            Resolution = -1;
        }

        public QSTSFD_Radial(
            RadialType radial,
            in float4 primaryAxisAngle,
            in float secondaryAngle,
            in float3 rotationCenter,
            float lerpLength,
            int resolution)
        {
            Type = radial;
            PrimaryAxisAngle = primaryAxisAngle;
            SecondOrderAngle = secondaryAngle;
            RotationCenter = rotationCenter;

            MaxLerpLength = lerpLength;
            Resolution = resolution;
        }

        public float4 PrimaryAxisAngle { get; private set; }
        public float SecondOrderAngle { get; private set; }
        public float3 RotationCenter { get; private set; }

        public int Resolution { get; set; }
        public float MaxLerpLength { get; set; }

        public bool IsRotationLerp
        {
            get
            {
                return Type == RadialType.FirstOrderRotation ||
                    Type == RadialType.SecondOrderRotation;
            }
        }

        public override string ToString()
        {
            return $"{MaxLerpLength:F2} {RotationCenter:F2} {Resolution} {PrimaryAxisAngle:F2}";
        }
    }
}