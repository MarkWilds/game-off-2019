using DefaultEcs;
using DefaultEcs.System;
using game.ECS.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.ECS.Systems
{
    [With(typeof(ScreenWeapon), typeof(Transform2D), typeof(Texture2DResources))]
    public class WeaponScreenRenderer : AEntitySystem<GameTime>
    {
        private readonly SpriteBatch spriteBatch;
        private readonly EntitySet cameraEntitySet;

        public WeaponScreenRenderer(World world, SpriteBatch spriteBatch) : base(world)
        {
            this.spriteBatch = spriteBatch;
            cameraEntitySet = world.GetEntities()
                .With(typeof(Transform2D))
                .With(typeof(Camera))
                .Build();
        }

        protected override void PreUpdate(GameTime state)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }

        protected override void Update(GameTime state, in Entity entity)
        {
            var camera = cameraEntitySet.GetEntities()[0];
            ref var cameraData = ref camera.Get<Camera>();
            
            ref var transform = ref entity.Get<Transform2D>();
            ref var screenWeapon = ref entity.Get<ScreenWeapon>();
            ref var texture2DDictionary = ref entity.Get<Texture2DResources>();

            var texture = texture2DDictionary.textures[screenWeapon.resourceName];

            var offsetPosition = transform.position;
            offsetPosition.X -= texture.Width / 2.0f + cameraData.bobFactor * screenWeapon.horizontalMoveFactor;
            offsetPosition.Y -= texture.Height / 2.0f + cameraData.bobFactor * screenWeapon.verticalMoveFactor;
            
            spriteBatch.Draw(texture, offsetPosition, Color.White);
        }

        protected override void PostUpdate(GameTime state)
        {
            spriteBatch.End();
        }
    }
}