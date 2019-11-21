using System;
using System.Collections.Generic;
using System.Linq;
using DefaultEcs;
using DefaultEcs.System;
using game.ECS.Components;
using game.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace game.ECS.Systems
{
    [With(typeof(Map), typeof(MapRenderData))]
    public class MapRendererSystem : AEntitySystem<GameTime>
    {
        private readonly SpriteBatch spriteBatch;
        private readonly EntitySet cameraEntitySet;
        private readonly float[] zBuffer;

        private readonly int screenWidth;
        private readonly int screenHeight;
        private int ShadeFactor;

        public MapRendererSystem(World world, int width, int height, SpriteBatch batch)
            : base(world)
        {
            cameraEntitySet = world.GetEntities()
                .With(typeof(Transform2D))
                .With(typeof(Camera))
                .Build();

            spriteBatch = batch;
            screenWidth = width;
            screenHeight = height;
            
            var blankTexture = new Texture2D(batch.GraphicsDevice, 1, 1);
            blankTexture.SetData(new[]{Color.White});
            
            zBuffer = new float[width];
        }

        protected override void Update(GameTime state, in Entity entity)
        {
            var camera = cameraEntitySet.GetFirst();

            ref var mapData = ref entity.Get<Map>();
            ref var mapRendererData = ref entity.Get<MapRenderData>();
            ShadeFactor = mapRendererData.darknessFactor;
            
            ref var cameraTransform = ref camera.Get<Transform2D>();
            ref var cameraData = ref camera.Get<Camera>();

            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            for (int i = 0; i < zBuffer.Length; i++)
                zBuffer[i] = float.MaxValue;

            RenderMap(in mapData, "walls1", in cameraTransform, in cameraData);
            RenderTileProps(in mapData, in cameraTransform.position, cameraTransform.orientation, in cameraData);

            spriteBatch.End();
        }

        private void RenderMap(in Map map, string wallsLayer, in Transform2D camTransform, in Camera camera)
        {
            TmxMap mapData = map.Data;
            int cellSize = mapData.TileWidth;
            
            float cameraAngle = camTransform.orientation * (float) (Math.PI / 180.0f);
            float fov = (float) (camera.fov * Math.PI / 180.0f);
            float halfFov = fov / 2;
            
            float columnDeltaAngle = fov / screenWidth;
            float columnStartAngle = cameraAngle - halfFov;
            float focalLength = (float) (screenWidth / 2.0 / Math.Tan(halfFov));

            float playerHeight = cellSize / 2.0f + camera.bobFactor;
            int projectionPlaneCenterHeight = screenHeight / 2 + camera.pitch;

            // draw all wallslices
            RayCaster.HitData castData = default;
            var tileHitPredicate = GetIsTileOccupiedFunction(mapData, wallsLayer);
            for (int column = 0; column < screenWidth; column++)
            {
                float currentAngle = columnStartAngle + column * columnDeltaAngle;
                float angleViewSpace = currentAngle - cameraAngle;

                if (!RayCaster.RayIntersectsGrid(camTransform.position, currentAngle, cellSize, ref castData,
                    tileHitPredicate))
                    continue;
                
                // fix fisheye for distance, get slice height and set the wallbuffer
                float straightDistance = (float) (castData.rayLength * Math.Cos(angleViewSpace));
                float rightTriangleRatio = focalLength / straightDistance;
                int wallHeight = (int) (cellSize * rightTriangleRatio);
                
                // based on player height and distance to wall we offset the wall
                int bottomOfWall = (int) (rightTriangleRatio * playerHeight + projectionPlaneCenterHeight);
                int topOfWall = bottomOfWall - wallHeight;

                zBuffer[column] = straightDistance;

                RenderFloor(in map, "floor1", column, cellSize, topOfWall, wallHeight,
                    playerHeight, currentAngle, projectionPlaneCenterHeight, focalLength, in camTransform.position,
                    cameraAngle);
                
                RenderCeiling(in map, "ceiling1", column, cellSize, topOfWall, wallHeight,
                    playerHeight, currentAngle, projectionPlaneCenterHeight, focalLength, in camTransform.position,
                    cameraAngle);

                // get the texture slice
                int tileIndex = (int) (castData.tileCoordinates.Y * mapData.Width + castData.tileCoordinates.X);
                TmxLayer wallLayer = mapData.Layers[wallsLayer];
                TmxLayerTile tile = wallLayer.Tiles[tileIndex];
                TmxTileset tileset = GetTilesetForTile(mapData, tile);
                if (tileset == null)
                    continue;

                // get drawing rectangles
                Rectangle wallRectangle = new Rectangle(column, topOfWall, 1, wallHeight + 1);
                Rectangle textureRectangle = GetSourceRectangleForTile(tileset, tile);

                textureRectangle.X =
                    (int) (textureRectangle.X + (textureRectangle.Width * castData.cellFraction) % cellSize);
                textureRectangle.Width = 1;

                // get texture tint
                float dot = Vector2.Dot(castData.normal, Vector2.UnitY);
                Color lightingTint = Math.Abs(dot) > 0.9f ? Color.Gray : Color.White;
                lightingTint.R = (byte) (Math.Min(255, lightingTint.R * ShadeFactor / straightDistance));
                lightingTint.G = (byte) (Math.Min(255, lightingTint.G * ShadeFactor / straightDistance));
                lightingTint.B = (byte) (Math.Min(255, lightingTint.B * ShadeFactor / straightDistance));

                spriteBatch.Draw(map.Textures[tileset], wallRectangle, textureRectangle, lightingTint);
            }
        }

        private void RenderFloor(in Map map, string layerName, int column, int cellSize, int topOfWall, int wallHeight,
            float playerHeight, float currentAngle, int projectionPlaneCenter, 
            float focalLength, in Vector2 camPosition, float cameraAngle)
        {
            TmxMap mapData = map.Data;
            TmxLayer layer = mapData.Layers[layerName];
            int bottomOfWall = topOfWall + wallHeight;
            for (int row = bottomOfWall; row < screenHeight; row++)
            {
                float ratio = playerHeight / (row - projectionPlaneCenter);
                var diagonalDistance = focalLength * ratio / Math.Cos(currentAngle - cameraAngle);

                double xEnd = Math.Cos(currentAngle) * diagonalDistance + camPosition.X;
                double yEnd = Math.Sin(currentAngle) * diagonalDistance + camPosition.Y;

                int cellX = (int) Math.Floor(xEnd / cellSize);
                int cellY = (int) Math.Floor(yEnd / cellSize);

                if (cellX < 0 || cellX >= mapData.Width ||
                    cellY < 0 || cellY >= mapData.Height)
                    continue;

                double cellXFraction = xEnd % cellSize / cellSize;
                double cellYFraction = yEnd % cellSize / cellSize;

                // get the texture slice
                var tileIndex = cellY * mapData.Width + cellX;
                var tile = layer.Tiles[tileIndex];
                var tileset = GetTilesetForTile(mapData, tile);
                if (tileset == null)
                    continue;

                // get drawing rectangles
                Rectangle groundPixel = new Rectangle(column, row, 1, 1);
                Rectangle texture = GetSourceRectangleForTile(tileset, tile);

                texture.X = (int) (texture.X + (texture.Width * cellXFraction) % cellSize);
                texture.Y = (int) (texture.Y + (texture.Height * cellYFraction) % cellSize);
                texture.Width = 1;
                texture.Height = 1;

                Color groundTint = Color.White;
                groundTint.R = (byte) (Math.Min(255, groundTint.R * ShadeFactor / diagonalDistance));
                groundTint.G = (byte) (Math.Min(255, groundTint.G * ShadeFactor / diagonalDistance));
                groundTint.B = (byte) (Math.Min(255, groundTint.B * ShadeFactor / diagonalDistance));
                spriteBatch.Draw(map.Textures[tileset], groundPixel, texture, groundTint);
            }
        }

        private void RenderCeiling(in Map map, string layerName, int column, int cellSize, int topOfWall, int wallHeight,
            float playerHeight, float currentAngle, int projectionPlaneCenter, 
            float focalLength, in Vector2 camPosition, float cameraAngle)
        {
            TmxMap mapData = map.Data;
            TmxLayer layer = mapData.Layers[layerName];
            for (int row = topOfWall; row > 0; row--)
            {
                float ratio = (cellSize - playerHeight) / (float) (projectionPlaneCenter - row);
                var diagonalDistance = focalLength * ratio / Math.Cos(currentAngle - cameraAngle);

                double xEnd = Math.Cos(currentAngle) * diagonalDistance + camPosition.X;
                double yEnd = Math.Sin(currentAngle) * diagonalDistance + camPosition.Y;

                int cellX = (int) xEnd / cellSize;
                int cellY = (int) Math.Floor(yEnd / cellSize);

                if (cellX < 0 || cellX >= mapData.Width ||
                    cellY < 0 || cellY >= mapData.Height)
                    continue;

                double cellXFraction = xEnd % cellSize / cellSize;
                double cellYFraction = yEnd % cellSize / cellSize;

                // get the texture slice
                var tileIndex = cellY * mapData.Width + cellX;
                var tile = layer.Tiles[tileIndex];
                var tileset = GetTilesetForTile(mapData, tile);
                if (tileset == null)
                    continue;

                // get drawing rectangles
                Rectangle groundPixel = new Rectangle(column, row, 1, 1);
                Rectangle texture = GetSourceRectangleForTile(tileset, tile);

                texture.X = (int) (texture.X + (texture.Width * cellXFraction) % cellSize);
                texture.Y = (int) (texture.Y + (texture.Height * cellYFraction) % cellSize);
                texture.Width = 1;
                texture.Height = 1;

                Color groundTint = Color.White;
                groundTint.R = (byte) (Math.Min(255, groundTint.R * ShadeFactor / diagonalDistance));
                groundTint.G = (byte) (Math.Min(255, groundTint.G * ShadeFactor / diagonalDistance));
                groundTint.B = (byte) (Math.Min(255, groundTint.B * ShadeFactor / diagonalDistance));
                spriteBatch.Draw(map.Textures[tileset], groundPixel, texture, groundTint);
            }
        }

        private void RenderObjectProps(in Map map, in Vector2 position, float orientation, in Camera camera)
        {
            TmxObjectGroup objectGroup = map.Data.ObjectGroups["objects"];

            var objectList = objectGroup.Objects.Where(o => o.Type == "prop").ToList();
            objectList.Sort((i1, i2) => 
                (int) ((i2.X * i2.X + i2.Y * i2.Y) - (i1.X * i1.X + i1.Y * i1.Y)));

            foreach (var objectProp in objectList)
            {
                
            }
        }

        private void RenderTileProps(in Map map, in Vector2 position, float orientation, in Camera camera)
        {
            int cellSize = map.Data.TileWidth;
            TmxLayer propsLayer = map.Data.Layers["props"];
            List<TmxLayerTile> propTiles = propsLayer.Tiles.Where(t => t.Gid > 0).ToList();
            int halfCellSize = cellSize / 2;

            var pos = position;
            var comparer = Comparer<TmxLayerTile>.Create((i1, i2) =>
            {
                Vector2 sprite1Position = new Vector2(i1.X * cellSize + halfCellSize,
                    i1.Y * cellSize + halfCellSize);
                Vector2 sprite2Position = new Vector2(i2.X * cellSize + halfCellSize,
                    i2.Y * cellSize + halfCellSize);

                return (int) ((sprite2Position - pos).LengthSquared() - (sprite1Position - pos).LengthSquared());
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

                RenderSprite(cellSize, in spritePosition, propTexture, ref source,
                    position, orientation, in camera);
            }
        }

        private void RenderSprite(int cellSize, in Vector2 spritePosition, Texture2D texture, ref Rectangle source,
            Vector2 cameraPosition, float orientation, in Camera camera)
        {
            float playerHeight = cellSize / 2.0f + camera.bobFactor;
            float fov = (float) (camera.fov * Math.PI / 180.0f);

            int projectionPlaneCenterHeight = screenHeight / 2 + camera.pitch;
            int projectionPlaneCenterWidth = screenWidth / 2;
            
            float halfFov = fov / 2;
            float cameraAngle = orientation * (float) (Math.PI / 180.0f);
            float focalLength = projectionPlaneCenterWidth / (float) Math.Tan(halfFov);

            Vector2 cameraForward = new Vector2((float) Math.Cos(cameraAngle), (float) Math.Sin(cameraAngle));
            Vector2 positionViewSpace = spritePosition - cameraPosition;

            if (Vector2.Dot(cameraForward, positionViewSpace) <= 0)
                return;

            float angleViewSpace = (float) Math.Atan2(positionViewSpace.Y, positionViewSpace.X) - cameraAngle;
            float correctedDistance = (float) (positionViewSpace.Length() * Math.Cos(angleViewSpace));

            float rightTriangleRatio = focalLength / correctedDistance;
            int bottomOfSpriteOnScreen = (int) (rightTriangleRatio * playerHeight + projectionPlaneCenterHeight);

            int spriteScreenSize = (int) (source.Width * rightTriangleRatio);
            int spriteScreenPosition = (int) (projectionPlaneCenterWidth + focalLength * Math.Tan(angleViewSpace));

            // draw slices for sprite
            int halfSprite = spriteScreenSize / 2;
            int startPosition = spriteScreenPosition - halfSprite;
            int endPosition = spriteScreenPosition + halfSprite;
            int tileStart = source.X;
            
            if (endPosition < 0 || startPosition >= screenWidth)
                return;

            if (startPosition < 0)
                startPosition = 0;

            bool noOffsetNeeded = false;
            if (endPosition >= screenWidth)
            {
                endPosition = screenWidth;
                noOffsetNeeded = true;
            }

            int spriteSizeRange = endPosition - startPosition;
            float spritePart = source.Width / (float) spriteScreenSize;
            float sourceOffset = noOffsetNeeded ? 0 : source.Width - spriteSizeRange / (float)spriteScreenSize * source.Width;
            
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
                    new Rectangle(screenColumn, bottomOfSpriteOnScreen - spriteScreenSize, 1, spriteScreenSize),
                    source, lightingTint);
            }
        }

        private Predicate<RayCaster.HitData> GetIsTileOccupiedFunction(TmxMap data, string layerName)
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

        private static TmxTileset GetTilesetForTile(TmxMap data, TmxLayerTile tile)
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