﻿using System.Collections.Generic;
using System.IO;
using DefaultEcs;
using DefaultEcs.Resource;
using game.Components;
using game.Resource.Resources;
using Microsoft.Xna.Framework;
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
            ref var map = ref entity.Get<Map>();
            map.Data = resource.TmxMap;
            map.Textures = new Dictionary<TmxTileset, Texture2D>();
            map.physicsWorld = new Humper.World(map.Data.Width * map.Data.TileWidth, 
                map.Data.Height * map.Data.TileHeight);

            CreateColliders(map);

            foreach (TmxTileset tileset in map.Data.Tilesets)
            {
                string pathToResource = Path.Combine(tilesetsPath, Path.GetFileName(tileset.Image.Source));
                if (!File.Exists(pathToResource))
                    continue;

                using (FileStream stream = new FileStream(pathToResource, FileMode.Open))
                {
                    Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);
                    map.Textures.Add(tileset, texture);
                }
            }
        }

        private void CreateColliders(Map map, string collisionLayer = "collision")
        {
            var data = map.Data;
            TmxLayer layer = data.Layers[collisionLayer];

            for (int y = 0; y < data.Height; y++)
            {
                for (int x = 0; x < data.Width; x++)
                {
                    TmxLayerTile tile = layer.Tiles[y * data.Width + x];
                    if (RaycastRenderer.GetTilesetForTile(data, tile) == null)
                        continue;
                    
                    map.physicsWorld.Create(x * data.TileWidth, y * data.TileHeight,
                        data.TileWidth, data.TileHeight);
                }
            }
        }
    }
}