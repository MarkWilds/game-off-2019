using DefaultEcs;
using DefaultEcs.System;
using game.ECS.Components;
using game.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.ECS.Systems
{
    [With(typeof(Map))]
    [With(typeof(MapRenderData))]
    public class SkyRendererSystem : AEntitySystem<GameTime>
    {
        private readonly SpriteBatch spriteBatch;
        private readonly EntitySet cameraEntitySet;

        private readonly Texture2D blankTexture;

        private readonly int startHeight;

        private Rectangle destination;
        private Rectangle skySource;
        private Rectangle cloudsSource;

        private const int CloudDirection = -1;
        private const int CloudSpeedMultiplier = 2;

        public SkyRendererSystem(World world, SpriteBatch spriteBatch, Texture2D blank, int width, int height) : base(world)
        {
            this.spriteBatch = spriteBatch;
            cameraEntitySet = world.GetEntities()
                .With(typeof(Transform2D))
                .With(typeof(Camera))
                .Build();

            skySource = new Rectangle();
            cloudsSource = new Rectangle();

            blankTexture = blank;

            startHeight = height / 2;
            destination = new Rectangle(0, 0, width, startHeight);
        }

        protected override void Update(GameTime state, in Entity entity)
        {
            ref var mapRenderData = ref entity.Get<MapRenderData>();
            switch (mapRenderData.skyType)
            {
                case SkyType.clouds:
                    if (!entity.Has<Texture2DResources>())
                        return;
                    
                    DrawClouds(state, in entity);
                    break;
                case SkyType.solid:
                    DrawSolid(state, in entity);
                    break;
            }
        }

        private void DrawSolid(GameTime state, in Entity entity)
        {
            ref var mapRenderData = ref entity.Get<MapRenderData>();
            
            spriteBatch.Begin();

            spriteBatch.Draw(blankTexture, destination, mapRenderData.skyColor);
            
            spriteBatch.End();
        }

        private void DrawClouds(GameTime state, in Entity entity)
        {
            ref var texture2DDictionary = ref entity.Get<Texture2DResources>();
            var sky = texture2DDictionary.textures["Sprites/sky"];
            var clouds = texture2DDictionary.textures["Sprites/clouds"];

            var camera = cameraEntitySet.GetFirst();
            ref var transform = ref camera.Get<Transform2D>();
            ref var cameraData = ref camera.Get<Camera>();

            var orientationOffset = transform.orientation / (180.0 / 3);

            skySource.X = (int) (orientationOffset * sky.Width);
            skySource.Width = sky.Width;
            skySource.Height = sky.Height;

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            destination.Height = startHeight - cameraData.pitch;
            spriteBatch.Draw(sky, destination, skySource, Color.White);

            double cloudMovingOffset = (state.TotalGameTime.TotalSeconds * CloudDirection * CloudSpeedMultiplier / 2) %
                                       clouds.Width;

            cloudsSource.Y = 0;
            cloudsSource.X = (int) (orientationOffset * clouds.Width + cloudMovingOffset);
            cloudsSource.Width = clouds.Width;
            cloudsSource.Height = clouds.Height;
            spriteBatch.Draw(clouds, destination, cloudsSource, Color.White);

            cloudMovingOffset = (state.TotalGameTime.TotalSeconds * CloudDirection * CloudSpeedMultiplier) %
                                clouds.Width;

            cloudsSource.Y -= 96;
            cloudsSource.X = (int) (orientationOffset * clouds.Width + cloudMovingOffset +
                                    CloudDirection * clouds.Width / 2.0);

            spriteBatch.Draw(clouds, destination, cloudsSource, Color.White);

            spriteBatch.End();
        }
    }
}