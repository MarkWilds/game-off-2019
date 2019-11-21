﻿using System;
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

        public override void HandleInput()
        {
            if (!IsTransitioning && Input.Input.IsKeyPressed(Keys.Escape))
            {
                ScreenManager.Publish(new PlaySound(){soundName = "Sfx/Close"});
                ExitScreen();
            }
        }

        public override void Draw(GameTime gameTime)
        {
            if (ScreenState == ScreenState.TransitionOn || ScreenState == ScreenState.Active)
                ScreenManager.FadeBackBufferToColor((int)((1 - TransitionPosition) * 150), Color.Black);
            
            if(ScreenState == ScreenState.TransitionOff)
                ScreenManager.FadeBackBufferToColor((int)(150 - TransitionPosition * 150), Color.Black);
        }
    }
}