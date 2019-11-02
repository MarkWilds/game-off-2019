using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace game.World
{
    public class Map
    {
        public TmxMap Data { get; private set; }
        public Dictionary<TmxTileset, Texture2D> Textures { get; private set; }

        private Map(TmxMap data, Dictionary<TmxTileset, Texture2D> textures)
        {
            Data = data;
            Textures = textures;
        }

        public Func<RayCaster.HitData, bool> GetIsTileOccupiedFunction(string layerName)
        {
            if (!Data.Layers.Contains(layerName))
                throw new ArgumentException($"{layerName} does not exist in this map");

            return hitData =>
            {
                Vector2 coordinates = hitData.tileCoordinates;
                if (coordinates.X < 0 || coordinates.X >= Data.Width ||
                    coordinates.Y < 0 || coordinates.Y >= Data.Height)
                    return false;

                int index = (int) (coordinates.Y * Data.Width + coordinates.X);
                TmxLayer wallLayer = Data.Layers[layerName];
                TmxLayerTile tile = wallLayer.Tiles[index];

                // if tileset is found it is solid
                return GetTilesetForTile(tile) != null;
            };
        }

        public static Map LoadTiledMap(GraphicsDevice graphicsDevice, string pathToMap)
        {
            TmxMap data = new TmxMap(pathToMap);
            Dictionary<TmxTileset, Texture2D> tilesetTextureMap = new Dictionary<TmxTileset, Texture2D>();

            string tilesheetFolder = @"Content/Tilesets";
            foreach (TmxTileset tileset in data.Tilesets)
            {
                string pathToResource = Path.Combine(tilesheetFolder, Path.GetFileName(tileset.Image.Source));
                if (!File.Exists(pathToResource))
                    continue;

                using (FileStream stream = new FileStream(pathToResource, FileMode.Open))
                {
                    Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);
                    tilesetTextureMap.Add(tileset, texture);
                }
            }

            return new Map(data, tilesetTextureMap);
        }

        public TmxTileset GetTilesetForTile(TmxLayerTile tile)
        {
            // TODO: fix tileset choosing, might have to refactor the tiledSharp lib for it
            // we now assume that tilesets are ordered in ascending order by gid
            int tilesetCount = Data.Tilesets.Count;
            for (int i = 0; i < tilesetCount; i++)
            {
                TmxTileset tileset = Data.Tilesets[tilesetCount - 1 - i];
                if (tile.Gid >= tileset.FirstGid)
                    return tileset;
            }

            return null;
        }

        public Rectangle GetSourceRectangleForTile(TmxTileset tileset, TmxLayerTile tile)
        {
            Rectangle source = new Rectangle();
            int tileWidth = tileset.TileWidth;
            int tileHeight = tileset.TileHeight;
            int tilesInHorizontalAxis = tileset.Image.Width.GetValueOrDefault() / tileWidth;

            // depending on the tile gid get the correct tile coordinates
            int tileIndex = tile.Gid - tileset.FirstGid;
            int xTilePos = tileIndex / tilesInHorizontalAxis;
            int yTilePos = tileIndex - xTilePos * tilesInHorizontalAxis;

            source.Width = tileWidth;
            source.Height = tileHeight;

            source.X = yTilePos * tileWidth;
            source.Y = xTilePos * tileHeight;

            return source;
        }

//        public Vector2 Move(Vector2 velocity, Entity entity, string collisionLayer = "collision")
//        {
//            TmxLayer layer = Data.Layers[collisionLayer];
//
//            Vector2 newPosition = entity.position + velocity;
//            Rectangle sweptBounds = new Rectangle((int) (newPosition.X - entity.Width / 2),
//                (int) (newPosition.Y - entity.Height / 2),
//                entity.Width, entity.Height);
//
//            // create swept rectangle
//            sweptBounds = Rectangle.Union(sweptBounds, entity.BoundingBox);
//
//            int minTileX = sweptBounds.Left / Data.TileWidth;
//            int minTileY = sweptBounds.Top / Data.TileHeight;
//
//            int maxTileX = sweptBounds.Right / Data.TileWidth + 1;
//            int maxTileY = sweptBounds.Bottom / Data.TileHeight + 1;
//
//            for (int y = minTileY; y < maxTileY; y++)
//            {
//                for (int x = minTileX; x < maxTileX; x++)
//                {
//                    if (x < 0 || x >= Data.Width ||
//                        y < 0 || y >= Data.Height)
//                        continue;
//
//                    TmxLayerTile tile = layer.Tiles[y * Data.Width + x];
//                    if (GetTilesetForTile(tile) == null)
//                        continue;
//
//                    Rectangle tileBounds = GetTileBounds(x, y);
//                    Rectangle intersection = Rectangle.Intersect(tileBounds, sweptBounds);
//
//                    if (intersection.Width < intersection.Height)
//                        velocity.X += -Math.Sign(velocity.X) * intersection.Width;
//                    else
//                        velocity.Y += -Math.Sign(velocity.Y) * intersection.Height;
//                }
//            }
//
//            return velocity;
//        }
    }
}