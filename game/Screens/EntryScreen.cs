using System;
using game.ECS.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace game.Screens
{
    public class EntryScreen : GameScreen
    {
        public EntryScreen()
        {
            TransitionOnTime = TimeSpan.FromSeconds(1);
            TransitionOffTime = TimeSpan.FromSeconds(1);
        }

        public override void HandleInput()
        {
            if (!IsTransitioning && Input.Input.IsKeyPressed(Keys.Space))
            {
                ExitScreen();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (ScreenState == ScreenState.TransitionOn || ScreenState == ScreenState.Active)
                ScreenManager.FadeBackBufferToColor((int)(200 + TransitionPosition * 55), Color.White);
            
            if(ScreenState == ScreenState.TransitionOff)
                ScreenManager.FadeBackBufferToColor((int)(200 - TransitionPosition * 200), Color.White);
        }
    }
}