using System;
using System.Collections.Generic;
using System.Linq;
using game.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace game
{
    public class RaycastRenderer
    {
        private Texture2D blankTexture;
        private float[] zBuffer;
        private const int ShadeFactor = 160;

        private readonly int width;
        private readonly int height;

        public RaycastRenderer(int width, int height, Texture2D blank)
        {
            this.width = width;
            this.height = height;
            blankTexture = blank;
            zBuffer = new float[this.width];
        }

        public void ClearDepthBuffer()
        {
            for (int i = 0; i < zBuffer.Length; i++)
            {
                zBuffer[i] = float.MaxValue;
            }
        }

        /// <summarY>
        /// Draws the map
        /// </summarY>
        /// <param name="map">The map to draw</param>
        /// <param name="position">The position to draw from</param>
        /// <param name="orientation">The rotation to draw from in degrees</param>
        public void RenderMap(SpriteBatch spriteBatch, Map map, Vector2 position, float orientation, int cellSize,
            string wallsLayer, in Camera camera)
        {
            var playerHeight = cellSize / 2;
            var fov = (float) (camera.fov * Math.PI / 180.0f);

            TmxMap mapData = map.Data;
            TmxLayer floorLayer = mapData.Layers["floor1"];
            int slices = width;
            float halfFov = fov / 2;
            float focalLength = slices / 2 / (float) Math.Tan(halfFov);
            float cameraAngle = orientation * (float) (Math.PI / 180.0f);

            float sliceAngle = fov / slices;
            float beginAngle = cameraAngle - halfFov;

            // draw ceiling and floor
            spriteBatch.Draw(blankTexture, new Rectangle(0, 0, width, height / 2 - camera.bobFactor),
                Color.FromNonPremultiplied(23, 14, 8, 255));
            spriteBatch.Draw(blankTexture, new Rectangle(0, height / 2 -camera.bobFactor, width, height / 2 + camera.bobFactor),
                Color.Black);

            // draw all wallslices
            for (int column = 0; column < slices; column++)
            {
                float angle = beginAngle + column * sliceAngle;

                RayCaster.HitData castData;
                if (!RayCaster.RayIntersectsGrid(position, angle, cellSize, out castData,
                    GetIsTileOccupiedFunction(mapData, wallsLayer)))
                    continue;

                // get the texture slice
                int tileIndex = (int) (castData.tileCoordinates.Y * mapData.Width + castData.tileCoordinates.X);
                TmxLayer wallLayer = mapData.Layers[wallsLayer];
                TmxLayerTile tile = wallLayer.Tiles[tileIndex];
                TmxTileset tileset = GetTilesetForTile(mapData, tile);
                if (tileset == null)
                    continue;

                // fix fisheye for distance and get slice height
                float distance = (float) (castData.rayLength * Math.Cos(angle - cameraAngle));
                int sliceHeight = (int) (cellSize * focalLength / distance);
                zBuffer[column] = distance;

                // get drawing rectangles
                var topOfWallSlice = height / 2 - sliceHeight / 2 - camera.bobFactor;
                Rectangle wallRectangle = new Rectangle(column, topOfWallSlice, 1, sliceHeight);
                Rectangle textureRectangle = GetSourceRectangleForTile(tileset, tile);

                textureRectangle.X =
                    (int) (textureRectangle.X + (textureRectangle.Width * castData.cellFraction) % cellSize);
                textureRectangle.Width = 1;

                // get texture tint
                float dot = Vector2.Dot(castData.normal, Vector2.UnitY);
                Color lightingTint = Math.Abs(dot) > 0.9f ? Color.Gray : Color.White;
                lightingTint.R = (byte) (Math.Min(255, lightingTint.R * ShadeFactor / distance));
                lightingTint.G = (byte) (Math.Min(255, lightingTint.G * ShadeFactor / distance));
                lightingTint.B = (byte) (Math.Min(255, lightingTint.B * ShadeFactor / distance));

                spriteBatch.Draw(map.Textures[tileset], wallRectangle, textureRectangle, lightingTint);

                // draw floor
                int startOfGroundSlice = topOfWallSlice + sliceHeight + 1;
                var projectionPlaneCenter = height / 2.0 - camera.bobFactor;
                for (int row = startOfGroundSlice; row < height; row++)
                {
                    var ratio = playerHeight / (row - projectionPlaneCenter);
                    var diagonalDistance = focalLength * ratio / Math.Cos(angle - cameraAngle);

                    double xEnd = Math.Cos(angle) * diagonalDistance + position.X;
                    double yEnd = Math.Sin(angle) * diagonalDistance + position.Y;

                    int cellX = (int) xEnd / cellSize;
                    int cellY = (int) Math.Floor(yEnd / cellSize);

                    if (cellX < 0 || cellX >= mapData.Width ||
                        cellY < 0 || cellY >= mapData.Height)
                        continue;
                    
                    double cellXFraction = xEnd % cellSize / cellSize;
                    double cellYFraction = yEnd % cellSize / cellSize;
                
                    // get the texture slice
                    tileIndex = cellY * mapData.Width + cellX;
                    tile = floorLayer.Tiles[tileIndex];
                    tileset = GetTilesetForTile(mapData, tile);
                    if (tileset == null)
                        continue;
                
                    // get drawing rectangles
                    Rectangle groundPixel = new Rectangle(column, row, 1, 1);
                    Rectangle texture = GetSourceRectangleForTile(tileset, tile);

                    texture.X = (int) (texture.X + (texture.Width * cellXFraction) % cellSize);
                    texture.Y = (int) (texture.Y + (texture.Height * cellYFraction) % cellSize);
                    texture.Width = 1;
                    texture.Height = 1;
                
                    spriteBatch.Draw(map.Textures[tileset], groundPixel, texture, Color.White);
                }
            }
        }

        public void RenderProps(Map map, SpriteBatch batch, int cellSize, 
            Vector2 position, float orientation, in Camera camera)
        {
            TmxLayer propsLayer = map.Data.Layers["props"];
            List<TmxLayerTile> propTiles = propsLayer.Tiles.Where(t => t.Gid > 0).ToList();
            int halfCellSize = cellSize / 2;

            var comparer = Comparer<TmxLayerTile>.Create((i1, i2) =>
            {
                Vector2 sprite1Position = new Vector2(i1.X * cellSize + halfCellSize,
                    i1.Y * cellSize + halfCellSize);
                Vector2 sprite2Position = new Vector2(i2.X * cellSize + halfCellSize,
                    i2.Y * cellSize + halfCellSize);

                return (int) ((sprite2Position - position).LengthSquared() - (sprite1Position - position).LengthSquared());
            });
            
            propTiles.Sort(comparer);
            foreach (TmxLayerTile propTile in propTiles)
            {
                TmxTileset tileset = GetTilesetForTile(map.Data, propTile);
                if (tileset == null)
                    continue;


                Texture2D propTexture = map.Textures[tileset];
                Vector2 spritePosition = new Vector2(propTile.X * cellSize + halfCellSize,
                    propTile.Y * cellSize + halfCellSize);
                Rectangle source = GetSourceRectangleForTile(tileset, propTile);

                RenderSprite(batch, spritePosition, propTexture, source,
                    position, orientation, in camera);
            }
        }

        /// <summary>
        /// Draws the sprite
        /// </summary>
        /// <param name="spriteBatch">batch to draw sprites with</param>
        /// <param name="position">position of the sprite</param>
        /// <param name="texture">texture to draw</param>
        /// <param name="camera">camera position</param>
        /// <param name="orientation">camera angle in degrees</param>
        public void RenderSprite(SpriteBatch spriteBatch, Vector2 position, Texture2D texture, Rectangle source,
            Vector2 playerPosition, float orientation, in Camera camera)
        {
            var fov = (float) (camera.fov * Math.PI / 180.0f);
            
            int slices = width;
            int halfSlice = slices / 2;
            float halfFov = fov / 2;
            float cameraAngle = orientation * (float) (Math.PI / 180.0f);
            float focalLength = halfSlice / (float) Math.Tan(halfFov);

            Vector2 cameraForward = new Vector2((float) Math.Cos(cameraAngle), (float) Math.Sin(cameraAngle));
            Vector2 spriteCameraSpace = position - playerPosition;

            if (Vector2.Dot(cameraForward, spriteCameraSpace) <= 0)
                return;

            float angleToSprite = (float) Math.Atan2(spriteCameraSpace.Y, spriteCameraSpace.X) - cameraAngle;
            float correctedDistance = (float) (spriteCameraSpace.Length() * Math.Cos(angleToSprite));
            int spriteSize = (int) (source.Width * focalLength / correctedDistance);
            int spritePosition = (int) (Math.Tan(angleToSprite) * focalLength + halfSlice);

            // draw slices for sprite
            int halfSprite = spriteSize / 2;
            int startPosition = spritePosition - halfSprite;
            int endPosition = spritePosition + halfSprite;
            int tileStart = source.X;
            
            if (endPosition < 0 || startPosition >= slices)
                return;

            if (startPosition < 0)
                startPosition = 0;

            bool noOffsetNeeded = false;
            if (endPosition >= slices)
            {
                endPosition = slices - 1;
                noOffsetNeeded = true;
            }

            int spriteSizeRange = endPosition - startPosition;
            float spritePart = source.Width / (float) spriteSize;
            float sourceOffset = noOffsetNeeded ? 0 : source.Width - spriteSizeRange / (float)spriteSize * source.Width;
            
            source.Width = (int) Math.Ceiling(spritePart);
            
            Color lightingTint = Color.White;
            lightingTint.R = (byte) (Math.Min(255, lightingTint.R * ShadeFactor / correctedDistance));
            lightingTint.G = (byte) (Math.Min(255, lightingTint.G * ShadeFactor / correctedDistance));
            lightingTint.B = (byte) (Math.Min(255, lightingTint.B * ShadeFactor / correctedDistance));
            for (int x = 0; x < spriteSizeRange; x++)
            {
                int screenColumn = startPosition + x;
                if (zBuffer[screenColumn] < correctedDistance)
                    continue;

                source.X = tileStart + (int) (sourceOffset + x * spritePart);

                spriteBatch.Draw(texture,
                    new Rectangle(screenColumn, height / 2 - halfSprite - camera.bobFactor, 1, spriteSize),
                    source, lightingTint);
            }
        }

        private Func<RayCaster.HitData, bool> GetIsTileOccupiedFunction(TmxMap data, string layerName)
        {
            if (!data.Layers.Contains(layerName))
                throw new ArgumentException($"{layerName} does not exist in this map");

            return hitData =>
            {
                Vector2 coordinates = hitData.tileCoordinates;
                if (coordinates.X < 0 || coordinates.X >= data.Width ||
                    coordinates.Y < 0 || coordinates.Y >= data.Height)
                    return false;

                int index = (int) (coordinates.Y * data.Width + coordinates.X);
                TmxLayer wallLayer = data.Layers[layerName];
                TmxLayerTile tile = wallLayer.Tiles[index];

                // if tileset is found it is solid
                return GetTilesetForTile(data, tile) != null;
            };
        }

        private Rectangle GetSourceRectangleForTile(TmxTileset tileset, TmxLayerTile tile)
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

        public static TmxTileset GetTilesetForTile(TmxMap data, TmxLayerTile tile)
        {
            // we now assume that tilesets are ordered in ascending order by gid
            int tilesetCount = data.Tilesets.Count;
            for (int i = 0; i < tilesetCount; i++)
            {
                TmxTileset tileset = data.Tilesets[tilesetCount - 1 - i];
                if (tile.Gid >= tileset.FirstGid)
                    return tileset;
            }

            return null;
        }
    }
}