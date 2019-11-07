using DefaultEcs;
using DefaultEcs.System;
using game.Components;
using Microsoft.Xna.Framework.Graphics;

namespace game.Systems
{
    [With(typeof(MapData))]
    public class MapRendererSystem : AEntitySystem<double>
    {
        private readonly SpriteBatch spriteBatch;
        private readonly RaycastRenderer renderer;

        private readonly EntitySet cameraBuilder;
        private Entity camera;
        
        private readonly int cellSize = 32;
        
        public MapRendererSystem(SpriteBatch batch, 
            Texture2D blankTexture, World world)
        : base(world)
        {
            cameraBuilder = world.GetEntities()
                .With(typeof(Transform2D))
                .With(typeof(CameraData))
                .Build();
                        
            spriteBatch = batch;
            renderer = new RaycastRenderer(batch.GraphicsDevice.Viewport, blankTexture);
        }

        protected override void PreUpdate(double state)
        {
            camera = cameraBuilder.GetEntities()[0];
            spriteBatch.Begin();
        }

        protected override void Update(double state, in Entity entity)
        {
            var mapData = entity.Get<MapData>();
            var cameraTransform = camera.Get<Transform2D>();
            var cameraData = camera.Get<CameraData>();
            
            renderer.ClearDepthBuffer();
            renderer.RenderMap(spriteBatch, mapData, cameraTransform.position, cameraTransform.angle,
                cellSize, "walls1", cameraData.fov);
            renderer.RenderProps(mapData, spriteBatch, cellSize, cameraTransform.position, 
                cameraTransform.angle, cameraData.fov);
        }

        protected override void PostUpdate(double state)
        {
            spriteBatch.End();
        }
    }
}