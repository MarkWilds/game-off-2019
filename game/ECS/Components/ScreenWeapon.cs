using Microsoft.Xna.Framework;

namespace game.ECS.Components
{
    public struct ScreenWeapon
    {
        public string resourceName;
        public Vector2 initialPosition;
        public float horizontalMoveFactor;
        public float verticalMoveFactor;
    }
}