using DefaultEcs.System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.ECS.Systems
{
    public class RenderTargetRenderer : ISystem<GameTime>
    {
        private readonly ISystem<GameTime> decoratedSystem;

        private readonly SpriteBatch spriteBatch;
        private readonly RenderTarget2D sceneTarget;
        private readonly Rectangle screenSizeRectangle;

        public bool IsEnabled
        {
            get => decoratedSystem.IsEnabled;
            set => decoratedSystem.IsEnabled = value;
        }

        public RenderTargetRenderer(int width, int height, GameWindow window, SpriteBatch batch,
            ISystem<GameTime> decorated)
        {
            spriteBatch = batch;
            decoratedSystem = decorated;

            screenSizeRectangle = GetPreferedScreenSizeRectangle(width, height, window);

            sceneTarget = new RenderTarget2D(batch.GraphicsDevice, width, height, false,
                SurfaceFormat.Color, DepthFormat.None,
                0, RenderTargetUsage.DiscardContents);
        }

        public void Dispose()
        {
            decoratedSystem?.Dispose();
        }

        public void Update(GameTime state)
        {
            spriteBatch.GraphicsDevice.SetRenderTarget(sceneTarget);

            decoratedSystem?.Update(state);

            spriteBatch.GraphicsDevice.SetRenderTarget(null);
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
            spriteBatch.Draw(sceneTarget, screenSizeRectangle, Color.White);
            spriteBatch.End();
        }

        private Rectangle GetPreferedScreenSizeRectangle(int width, int height, GameWindow window)
        {
            var windowWidth = window.ClientBounds.Width;
            var windowHeight = window.ClientBounds.Height;
            float outputAspect = windowWidth / (float) windowHeight;
            float preferredAspect = width / (float) height;

            if (outputAspect <= preferredAspect)
            {
                // output is taller than it is wider, bars on top/bottom
                int presentHeight = (int) ((windowWidth / preferredAspect) + 0.5f);
                int barHeight = (windowHeight - presentHeight) / 2;
                return new Rectangle(0, barHeight, windowWidth, presentHeight);
            }

            // output is wider than it is tall, bars left/right
            int presentWidth = (int) ((windowHeight * preferredAspect) + 0.5f);
            int barWidth = (windowWidth - presentWidth) / 2;
            return new Rectangle(barWidth, 0, presentWidth, windowHeight);
        }
    }
}