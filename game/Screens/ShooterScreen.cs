using System;
using System.Collections.Generic;
using System.Linq;
using game.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using TiledSharp;

namespace game.GameScreens
{
    public class ShooterScreen : IGameScreen
    {
        public ScreenManager ScreenManager { get; set; }

        private Map currentMap;
        private RaycastRenderer renderer;

        private Texture2D blankTexture;
        private Vector2 position = new Vector2(32 + 16, 64 + 16);
        private float movementSpeed = 128;
        private float angle;
        private int cellSize = 32;
        private MouseState previousState;

        public void Initialize(ContentManager contentManager)
        {
            blankTexture = contentManager.Load<Texture2D>("blank");
            renderer = new RaycastRenderer(ScreenManager.GraphicsDevice.Viewport, blankTexture, 60.0f);
            currentMap = Map.LoadTiledMap(ScreenManager.GraphicsDevice, "Content/maps/test_fps.tmx");

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Mouse.SetPosition(viewport.Width / 2, viewport.Height / 2);
            previousState = Mouse.GetState();
        }

        public void Update(GameTime gameTime)
        {
            if (InputManager.IsKeyPressed(Keys.F4))
            {
                ScreenManager.PopScreen();
            }

            MouseState currentState = Mouse.GetState();
            if (currentState != previousState)
            {
                float deltaX = currentState.X - previousState.X;
                angle += deltaX * 20.0f * (float) gameTime.ElapsedGameTime.TotalSeconds;
            }

            Viewport viewport = ScreenManager.GraphicsDevice.Viewport;
            Mouse.SetPosition(viewport.Width / 2, viewport.Height / 2);

            Vector2 forward = new Vector2((float) Math.Cos(angle * Math.PI / 180),
                (float) Math.Sin(angle * Math.PI / 180));
            Vector2 right = new Vector2(-forward.Y, forward.X);

            Vector2 movementDirection = forward * InputManager.VerticalAxis + right * InputManager.HorizontalAxis;

            if (movementDirection != Vector2.Zero)
            {
                movementDirection.Normalize();

                Vector2 velocity = movementDirection * movementSpeed * (float) gameTime.ElapsedGameTime.TotalSeconds;

                // do collision detection
                RayCaster.HitData hitData;
                float dirAngle = (float) Math.Atan2(velocity.Y, velocity.X);
                if (RayCaster.RayIntersectsGrid(position, dirAngle, 32, out hitData,
                    currentMap.GetIsTileOccupiedFunction("walls1"), 16))
                {
                    if (hitData.rayLength >= 0)
                    {
                        float vel = velocity.Length();
                        velocity.Normalize();
                    }
                }


//                player.position = position;
//                velocity = currentMap.Move(velocity, player);
                position += velocity;
            }
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            ScreenManager.GraphicsDevice.Clear(Color.CornflowerBlue);

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

        public void Dispose()
        {
        }
    }
}