using DefaultEcs;
using Microsoft.Xna.Framework;

namespace game.Components
{
    public struct Transform2D
    {
        public Vector2 position;
        public float angle;
    }

    public static partial class WorldExtensions
    {
        public static Transform2D CreateTransform2D(this World world, float x, float y, float angle)
        {
            return new Transform2D() {position = new Vector2(x, y), angle = angle};
        }
    }
}