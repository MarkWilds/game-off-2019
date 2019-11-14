using System.Collections.Generic;
using Humper;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace game.ECS.Components
{
    public struct Map
    {
        public TmxMap Data { get; set; }
        
        public Dictionary<TmxTileset, Texture2D> Textures { get; set; }

        public World physicsWorld;
    }
}