using System.Collections.Generic;
using game.Data;
using Humper;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace game.ECS.Components
{
    public struct Map
    {
        public TmxMap Data { get; set; }
        public Dictionary<TmxTileset, Texture2D> Textures { get; set; }
        public List<MapProp> MapPropList { get; set; }
        public World PhysicsWorld { get; set; }
    }
}