using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace game.Screens
{
    public class EntryScreen : GameScreen
    {
        public EntryScreen()
        {
            IsPopup = true;
            TransitionOnTime = TimeSpan.FromSeconds(0);
            TransitionOffTime = TimeSpan.FromSeconds(0.5);
        }

        public override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                if (!IsTransitioning && Input.Input.IsKeyPressed(Keys.Space))
                {
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