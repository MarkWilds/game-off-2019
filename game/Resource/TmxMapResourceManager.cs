using System.Collections.Generic;
using System.IO;
using DefaultEcs;
using DefaultEcs.Resource;
using game.Components;
using game.Resource.Resources;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace game.Resource
{
    public class TmxMapResourceManager : AResourceManager<string, DisposableTmxMap>
    {
        private readonly GraphicsDevice graphicsDevice;
        private readonly string tilesetsPath;

        public TmxMapResourceManager(GraphicsDevice graphicsDevice, string tilesetPath)
        {
            this.graphicsDevice = graphicsDevice;
            this.tilesetsPath = tilesetPath;
        }

        protected override DisposableTmxMap Load(string file)
        {
            var tmxMap = new TmxMap(file);
            return new DisposableTmxMap() {TmxMap = tmxMap};
        }

        protected override void OnResourceLoaded(in Entity entity, string info, DisposableTmxMap resource)
        {
            ref MapData mapData = ref entity.Get<MapData>();
            mapData.Data = resource.TmxMap;
            mapData.Textures = new Dictionary<TmxTileset, Texture2D>();

            foreach (TmxTileset tileset in mapData.Data.Tilesets)
            {
                string pathToResource = Path.Combine(tilesetsPath, Path.GetFileName(tileset.Image.Source));
                if (!File.Exists(pathToResource))
                    continue;

                using (FileStream stream = new FileStream(pathToResource, FileMode.Open))
                {
                    Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);
                    mapData.Textures.Add(tileset, texture);
                }
            }
        }
    }
}