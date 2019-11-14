using System;
using DefaultEcs;
using DefaultEcs.System;
using game.ECS.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.ECS.Systems
{
    [With(typeof(Texture2DDictionary))]
    [With(typeof(Map))]
    public class SkyRendererSystem : AEntitySystem<GameTime>
    {
        private readonly SpriteBatch spriteBatch;
        private readonly EntitySet cameraEntitySet;
        private readonly Rectangle destination;

        private Rectangle source;

        public SkyRendererSystem(World world, SpriteBatch spriteBatch, int width, int height) : base(world)
        {
            this.spriteBatch = spriteBatch;
            cameraEntitySet = world.GetEntities()
                .With(typeof(Transform2D))
                .With(typeof(Camera))
                .Build();

            source = new Rectangle();
            destination = new Rectangle(0,0, width, height / 2);
        }

        protected override void Update(GameTime state, in Entity entity)
        {
            var texture2DDictionary = entity.Get<Texture2DDictionary>();
            var sky = texture2DDictionary.textures["Sprites/sky"];
//            var clouds = texture2DDictionary.textures["Sprites/clouds"];
            
            var player = cameraEntitySet.GetEntities()[0];
            var transform = player.Get<Transform2D>();

            var skyOffset = transform.angle / (180.0 / 3);

            source.X = (int) (skyOffset * sky.Width);
            source.Width = sky.Width;
            source.Height = sky.Height;
            
            spriteBatch.Begin(samplerState: SamplerState.PointWrap);

            spriteBatch.Draw(sky, destination, source, Color.White);
//            spriteBatch.Draw(clouds, destination, source, Color.White);
            
            spriteBatch.End();
        }
    }
}