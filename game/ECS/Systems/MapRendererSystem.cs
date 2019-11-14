using DefaultEcs;
using DefaultEcs.System;
using game.ECS.Components;
using game.Raycasting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.ECS.Systems
{
    [With(typeof(Map))]
    public class MapRendererSystem : AEntitySystem<GameTime>
    {
        private readonly SpriteBatch spriteBatch;
        private readonly RaycastRenderer renderer;

        private readonly EntitySet cameraEntitySet;

        public MapRendererSystem(int width, int height, SpriteBatch batch, World world)
            : base(world)
        {
            cameraEntitySet = world.GetEntities()
                .With(typeof(Transform2D))
                .With(typeof(Camera))
                .Build();

            spriteBatch = batch;
            
            var blankTexture = new Texture2D(batch.GraphicsDevice, 1, 1);
            blankTexture.SetData(new[]{Color.White});
            
            renderer = new RaycastRenderer(width, height, blankTexture);
        }

        protected override void Update(GameTime state, in Entity entity)
        {
            var camera = cameraEntitySet.GetEntities()[0];

            var mapData = entity.Get<Map>();
            var cameraTransform = camera.Get<Transform2D>();
            var cameraData = camera.Get<Camera>();
            var cellSize = mapData.Data.TileWidth;
            
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            renderer.ClearDepthBuffer();
            
            renderer.RenderMap(spriteBatch, mapData, cameraTransform.position, cameraTransform.angle,
                cellSize, "walls1", in cameraData);
            renderer.RenderProps(mapData, spriteBatch, cellSize, cameraTransform.position,
                cameraTransform.angle, in cameraData);

            spriteBatch.End();
        }
    }
}