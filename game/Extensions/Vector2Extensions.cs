using Microsoft.Xna.Framework;

namespace game
{
    public static class Vector2Extensions
    {
        public static Vector2 Perp(this in Vector2 lhs)
        {
            return new Vector2(-lhs.Y, lhs.X);
        }

        public static void Turn90Degrees(this ref Vector2 lhs)
        {
            ref var x = ref lhs.X;
            lhs.X = -lhs.Y;
            lhs.Y = x;
        }
        
        public static float PerpDot(this in Vector2 lhs, in Vector2 rhs)
        {
            return -lhs.Y * rhs.X + lhs.X * rhs.Y;
        }
    }
}