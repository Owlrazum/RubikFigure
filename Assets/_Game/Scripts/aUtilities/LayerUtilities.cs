namespace Orazum.Utilities
{
    public static class LayerUtilities
    {
        public const int PLAYER_COLLISION_LAYER = 6;
        public const int PLAYER_COLLISION_LAYER_MASK = 1 << PLAYER_COLLISION_LAYER;

        public static bool IsInLayerMaskLayer(int layer)
        {
            if ((PLAYER_COLLISION_LAYER_MASK & (1 << layer)) != 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}