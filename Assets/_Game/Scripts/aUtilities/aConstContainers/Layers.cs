namespace Orazum.Constants
{
    public static class Layers
    {
        public static bool IsInLayerMaskLayer(int layer, int layerMask)
        {
            if ((layerMask & (1 << layer)) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public const int FigureLayer = 6;

        public const int SegmentPointsLayer = 7;
        public const int SegmentPointsLayerMask = 1 << SegmentPointsLayer;
    }
}