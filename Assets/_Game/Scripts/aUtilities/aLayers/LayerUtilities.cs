namespace Orazum.Utilities.ConstContainers
{
    public static class LayerUtilities
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

        public const int FIGURE_LAYER = 6;

        public const int SEGMENT_POINTS_LAYER = 7;
        public const int SEGMENT_POINTS_LAYER_MASK = 1 << SEGMENT_POINTS_LAYER;
    }
}