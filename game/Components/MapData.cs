using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace game.Components
{
    public struct MapData
    {
        public TmxMap Data { get; set; }
        
        public Dictionary<TmxTileset, Texture2D> Textures { get; set; }
    }
}