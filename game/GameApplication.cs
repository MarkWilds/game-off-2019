using System;
using System.Collections.Generic;
using System.Linq;
using game.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TiledSharp;

namespace game
{
    public class GameApplication : Game
    {
        private SpriteBatch spriteBatch;
        private GraphicsDeviceManager graphics;
        private Map currentMap;
        private RaycastRenderer renderer;

        private Texture2D blankTexture;
        private Vector2 position = new Vector2(32 + 16, 64 + 16);
        private float movementSpeed = 128;
        private float angle;
        private int cellSize = 32;

        public GameApplication()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1024, PreferredBackBufferHeight = 768
            };
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            blankTexture = Content.Load<Texture2D>("blank");
            renderer = new RaycastRenderer(GraphicsDevice.Viewport, blankTexture, 60.0f);
            currentMap = Map.LoadTiledMap(GraphicsDevice, "Content/maps/test_fps.tmx");

            Viewport viewport = GraphicsDevice.Viewport;
            Mouse.SetPosition(viewport.Width / 2, viewport.Height / 2);
        }

        protected override void Update(GameTime deltaTime)
        {
            InputManager.Update();
            var mouseDelta = InputManager.MouseAxisX;
            var deadzone = 2.0;
            if(Math.Abs(mouseDelta) > deadzone)
                angle += mouseDelta * 20.0f * (float) deltaTime.ElapsedGameTime.TotalSeconds;

            double angleRad = angle * Math.PI / 180;
            Vector2 forward = new Vector2((float) Math.Cos(angleRad),(float) Math.Sin(angleRad));
            Vector2 right = new Vector2(-forward.Y, forward.X);

            // basically a vector * matrix transformation
            Vector2 movementDirection = forward * InputManager.VerticalAxis + right * InputManager.HorizontalAxis;
            if (movementDirection.LengthSquared() > 0)
            {
                movementDirection.Normalize();

                Vector2 velocity = movementDirection * movementSpeed * (float) deltaTime.ElapsedGameTime.TotalSeconds;

                // do collision detection
                RayCaster.HitData hitData;
                float dirAngle = (float) Math.Atan2(velocity.Y, velocity.X);
                if (RayCaster.RayIntersectsGrid(position, dirAngle, 32, out hitData,
                    currentMap.GetIsTileOccupiedFunction("walls1"), 16))
                {
                    if (hitData.rayLength >= 0)
                        velocity.Normalize();
                }
                
//                velocity = currentMap.Move(velocity, position);
                position += velocity;
            }
        }

        protected override void Draw(GameTime deltaTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            renderer.ClearDepthBuffer();
            renderer.RenderMap(spriteBatch, currentMap, position, angle, cellSize, "walls1");

            RenderProps(spriteBatch);

            spriteBatch.End();
        }

        private void RenderProps(SpriteBatch batch)
        {
            TmxLayer propsLayer = currentMap.Data.Layers["props"];
            List<TmxLayerTile> propTiles = propsLayer.Tiles.Where(t => t.Gid > 0).ToList();

            foreach (TmxLayerTile propTile in propTiles)
            {
                TmxTileset tileset = currentMap.GetTilesetForTile(propTile);
                if (tileset == null)
                    continue;

                int halfCellSize = cellSize / 2;
                Texture2D propTexture = currentMap.Textures[tileset];
                Vector2 spritePosition = new Vector2(propTile.X * cellSize + halfCellSize,
                    propTile.Y * cellSize + halfCellSize);
                Rectangle source = currentMap.GetSourceRectangleForTile(tileset, propTile);

                renderer.RenderSprite(batch, spritePosition, propTexture, source,
                    position, angle);
            }
        }
    }
}