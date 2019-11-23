using System;
using game.ECS.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace game.Screens
{
    public class PauseScreen : GameScreen
    {
        public PauseScreen()
        {
            IsPopup = true;
            TransitionOnTime = TimeSpan.FromSeconds(0.3);
            TransitionOffTime = TimeSpan.FromSeconds(0.3);
        }

        public override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                if (!IsTransitioning && Input.Input.IsKeyPressed(Keys.Escape))
                {
                    ScreenManager.Publish(new PlaySound(){soundName = "Sfx/Close"});
                    ExitScreen();
                }
                ScreenManager.Game.IsMouseVisible = true;
            }
            else
            {
                ScreenManager.Game.IsMouseVisible = false;
            }
        }

        public override void Draw(GameTime gameTime)
        {
            ScreenManager.FadeBackBufferToColor(TransitionAlpha * 2/3, Color.Black);
        }
    }
}