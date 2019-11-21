using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.Data
{
    public struct MapProp
    {
        public Vector2 position;
        public float orientation;

        public Rectangle source;
        public Texture2D texture;

        public MapProp(int x, int y, in Rectangle src, float orien, Texture2D tex)
        {
            position = default;
            position.X = x;
            position.Y = y;
            
            orientation = orien;
            texture = tex;

            source = src;
        }
    }
}