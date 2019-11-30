using DefaultEcs;
using DefaultEcs.System;
using game.ECS.Components;
using game.StateMachine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.ECS.Systems
{
    [With(typeof(ScreenWeapon), typeof(Transform2D), typeof(Texture2DResources))]
    public class WeaponScreenRenderer : AEntitySystem<GameTime>
    {
        private readonly SpriteBatch spriteBatch;

        public WeaponScreenRenderer(World world, SpriteBatch spriteBatch) : base(world)
        {
            this.spriteBatch = spriteBatch;
        }

        protected override void PreUpdate(GameTime state)
        {
            spriteBatch.Begin(samplerState: SamplerState.PointClamp);
        }

        protected override void Update(GameTime state, in Entity entity)
        {
            ref var weaponState = ref entity.Get<IState>();
            weaponState.Update((float) state.ElapsedGameTime.TotalSeconds);
            
            ref var transform = ref entity.Get<Transform2D>();
            ref var screenWeapon = ref entity.Get<ScreenWeapon>();
            ref var textures = ref entity.Get<Texture2DResources>();
            
            spriteBatch.Draw(textures.textures[screenWeapon.resourceName], transform.position, Color.White);
        }

        protected override void PostUpdate(GameTime state)
        {
            spriteBatch.End();
        }
    }
}