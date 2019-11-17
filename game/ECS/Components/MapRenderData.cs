using Microsoft.Xna.Framework;

namespace game.ECS.Components
{
    public struct MapRenderData
    {
        public int darknessFactor;
        public SkyType skyType;
        public Color skyColor;
    }

    public enum SkyType
    {
        clouds = 0,
        solid
    }
}