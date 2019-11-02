using System;
using game.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace game
{
    public class RaycastRenderer
    {
        private readonly float fov;

        private Texture2D blankTexture;
        private Viewport viewport;
        private float[] zBuffer;

        public RaycastRenderer(Viewport view, Texture2D blank, float fov)
        {
            viewport = view;
            blankTexture = blank;
            this.fov = (float) (fov * Math.PI / 180.0f);
            zBuffer = new float[viewport.Width];
        }

        public void ClearDepthBuffer()
        {
            for (int i = 0; i < zBuffer.Length; i++)
            {
                zBuffer[i] = float.MaxValue;
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
            Vector2 camera, float orientation)
        {
            int slices = viewport.Width;
            int halfSlice = slices / 2;
            float halfFov = fov / 2;
            float cameraAngle = orientation * (float) (Math.PI / 180.0f);
            float focalLength = halfSlice / (float) Math.Tan(halfFov);

            Vector2 cameraForward = new Vector2((float) Math.Cos(cameraAngle), (float) Math.Sin(cameraAngle));
            Vector2 spriteCameraSpace = position - camera;

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
            for (int x = 0; x < spriteSizeRange; x++)
            {
                int screenColumn = startPosition + x;
                if (zBuffer[screenColumn] < correctedDistance)
                    continue;

                source.X = tileStart + (int) (sourceOffset + x * spritePart);

                spriteBatch.Draw(texture,
                    new Rectangle(screenColumn, viewport.Height / 2 - halfSprite, 1, spriteSize),
                    source, Color.White);
            }
        }

        /// <summarY>
        /// Draws the map
        /// </summarY>
        /// <param name="map">The map to draw</param>
        /// <param name="camera">The position to draw from</param>
        /// <param name="orientation">The rotation to draw from in degrees</param>
        public void RenderMap(SpriteBatch spriteBatch, Map map, Vector2 camera, float orientation, int cellSize,
            string wallsLayer)
        {
            TmxMap mapData = map.Data;
            int slices = viewport.Width;
            float halfFov = fov / 2;
            float focalLength = slices / 2 / (float) Math.Tan(halfFov);
            float cameraAngle = orientation * (float) (Math.PI / 180.0f);

            float sliceAngle = fov / slices;
            float beginAngle = cameraAngle - halfFov;

            // draw ceiling and floor
            spriteBatch.Draw(blankTexture, new Rectangle(0, 0, viewport.Width, viewport.Height / 2),
                Color.FromNonPremultiplied(23, 14, 8, 255));
            spriteBatch.Draw(blankTexture, new Rectangle(0, viewport.Height / 2, viewport.Width, viewport.Height / 2),
                Color.DarkKhaki);

            // draw all wallslices
            for (int x = 0; x < slices; x++)
            {
                float angle = beginAngle + x * sliceAngle;

                RayCaster.HitData castData;
                if (!RayCaster.RayIntersectsGrid(camera, angle, cellSize, out castData,
                    map.GetIsTileOccupiedFunction(wallsLayer)))
                    continue;

                // get the texture slice
                int tileIndex = (int) (castData.tileCoordinates.Y * mapData.Width + castData.tileCoordinates.X);
                TmxLayer wallLayer = mapData.Layers[wallsLayer];
                TmxLayerTile tile = wallLayer.Tiles[tileIndex];
                TmxTileset tileset = map.GetTilesetForTile(tile);
                if (tileset == null)
                    continue;

                // fix fisheye for distance and get slice height
                float distance = (float) (castData.rayLength * Math.Cos(angle - cameraAngle));
                int sliceHeight = (int) (cellSize * focalLength / distance);
                zBuffer[x] = distance;

                // get drawing rectangles
                Rectangle wallRectangle = new Rectangle(x, viewport.Height / 2 - sliceHeight / 2, 1, sliceHeight);
                Rectangle textureRectangle = map.GetSourceRectangleForTile(tileset, tile);

                textureRectangle.X =
                    (int) (textureRectangle.X + (textureRectangle.Width * castData.cellFraction) % cellSize);
                textureRectangle.Width = 1;

                // get texture tint
                float dot = Vector2.Dot(castData.normal, Vector2.UnitY);
                Color lightingTint = Math.Abs(dot) > 0.9f ? Color.Gray : Color.White;

                spriteBatch.Draw(map.Textures[tileset], wallRectangle, textureRectangle, lightingTint);
            }
        }
    }
}