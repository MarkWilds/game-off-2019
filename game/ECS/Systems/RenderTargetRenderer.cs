using DefaultEcs.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.ECS.Systems
{
    public class RenderTargetRenderer : ISystem<GameTime>
    {
        private readonly ISystem<GameTime> decorated;

        private readonly Rectangle preferedSourceRectangle;
        private readonly RenderTarget2D sceneTarget;

        private readonly SpriteBatch spriteBatch;

        public bool IsEnabled
        {
            get => decorated.IsEnabled;
            set => decorated.IsEnabled = value;
        }

        public RenderTargetRenderer(int width, int height, Rectangle preferedSource, SpriteBatch batch,
            ISystem<GameTime> decorated)
        {
            this.spriteBatch = batch;
            this.decorated = decorated;
            this.preferedSourceRectangle = preferedSource;

            sceneTarget = new RenderTarget2D(batch.GraphicsDevice, width, height, false,
                SurfaceFormat.Color, DepthFormat.None,
                0, RenderTargetUsage.DiscardContents);
        }

        public void Dispose()
        {
            decorated?.Dispose();
        }

        public void Update(GameTime state)
        {
            spriteBatch.GraphicsDevice.SetRenderTarget(sceneTarget);

            decorated?.Update(state);

            spriteBatch.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(sceneTarget, preferedSourceRectangle, Color.White);
            spriteBatch.End();
        }
    }
}