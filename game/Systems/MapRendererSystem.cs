using DefaultEcs;
using DefaultEcs.System;
using game.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.Systems
{
    [With(typeof(Map))]
    public class MapRendererSystem : AEntitySystem<double>
    {
        private readonly SpriteBatch spriteBatch;
        private readonly RaycastRenderer renderer;

        private readonly EntitySet cameraBuilder;
        private Entity camera;

        private readonly int screenWidth;
        private readonly int screenHeight;
        private readonly Rectangle preferedSourceRectangle;
        private RenderTarget2D sceneTarget;

        public MapRendererSystem(int width, int height, SpriteBatch batch,
            Texture2D blankTexture, Rectangle preferedSource, World world)
            : base(world)
        {
            cameraBuilder = world.GetEntities()
                .With(typeof(Transform2D))
                .With(typeof(Camera))
                .Build();

            preferedSourceRectangle = preferedSource;
            screenWidth = width;
            screenHeight = height;

            spriteBatch = batch;
            renderer = new RaycastRenderer(screenWidth, screenHeight, blankTexture);
            sceneTarget = new RenderTarget2D(batch.GraphicsDevice, screenWidth, screenHeight, false,
                SurfaceFormat.Color,
                DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
        }

        protected override void Update(double state, in Entity entity)
        {
            camera = cameraBuilder.GetEntities()[0];

            var mapData = entity.Get<Map>();
            var cameraTransform = camera.Get<Transform2D>();
            var cameraData = camera.Get<Camera>();
            var cellSize = mapData.Data.TileWidth;

            spriteBatch.GraphicsDevice.SetRenderTarget(sceneTarget);
            spriteBatch.Begin();

            renderer.ClearDepthBuffer();
            renderer.RenderMap(spriteBatch, mapData, cameraTransform.position, cameraTransform.angle,
                cellSize, "walls1", cameraData.fov);
            renderer.RenderProps(mapData, spriteBatch, cellSize, cameraTransform.position,
                cameraTransform.angle, cameraData.fov);

            spriteBatch.End();

            // draw scene to backbuffer
            spriteBatch.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(sceneTarget, preferedSourceRectangle, Color.White);
            spriteBatch.End();
        }
    }
}