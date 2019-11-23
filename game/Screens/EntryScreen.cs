using System;
using game.ECS.Events;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace game.Screens
{
    public class EntryScreen : GameScreen
    {
        private const string SubTitleText = "a monster!!!";
        private const string StartGameText = "Press the spacebar to start!";
        
        private Texture2D titleTexture;
        private SpriteFont titleFont;
        
        private Vector2 startPosition = Vector2.One;
        private Vector2 endPosition = Vector2.One;
        private Rectangle titleDestination;
        
        private Vector2 subTitleStartPosition = Vector2.One;
        private Vector2 subTitleEndPosition = Vector2.One;
        private Vector2 subTitlePosition = Vector2.One;
        
        private Vector2 startGameTextPos = Vector2.One;

        private double normalizedSin;
        
        public EntryScreen()
        {
            IsPopup = true;
            TransitionOnTime = TimeSpan.FromSeconds(0.5);
            TransitionOffTime = TimeSpan.FromSeconds(0.3);
        }

        public override void LoadContent()
        {
            var viewport = ScreenManager.GraphicsDevice.Viewport;
            
            titleTexture = ScreenManager.Game.Content.Load<Texture2D>("Sprites/title");
            titleFont = ScreenManager.Game.Content.Load<SpriteFont>("Fonts/segoe");
            
            titleDestination.Width = (int) (titleTexture.Width / viewport.AspectRatio);
            titleDestination.Height = (int) (titleTexture.Height / viewport.AspectRatio);
            
            startPosition.X = viewport.Width - titleDestination.Width;
            startPosition.Y = -titleDestination.Height;

            endPosition.X = startPosition.X;
            endPosition.Y = 64 / viewport.AspectRatio;

            var subTitleDimensions = titleFont.MeasureString(SubTitleText);
            subTitleStartPosition.X = viewport.Width + subTitleDimensions.X;
            subTitleStartPosition.Y = endPosition.Y + titleDestination.Height;
            subTitleEndPosition.X = endPosition.X + 64 / viewport.AspectRatio;
            subTitleEndPosition.Y = subTitleStartPosition.Y - 48 / viewport.AspectRatio;

            var startGameTextDimensions = titleFont.MeasureString(StartGameText);
            startGameTextPos.X = (float) (viewport.Width / 2.0 - startGameTextDimensions.X / 2);
            startGameTextPos.Y = viewport.Height - (startGameTextDimensions.Y + 64 / viewport.AspectRatio);
        }

        public override void UnloadContent()
        {
            titleTexture.Dispose();
        }

        public override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                if (!IsTransitioning && Input.Input.IsKeyPressed(Keys.Space))
                {
                    ScreenManager.Publish(new PlaySound() {soundName = "Sfx/New"});
                    ExitScreen();
                }
                ScreenManager.Game.IsMouseVisible = true;
            }
            else
            {
                ScreenManager.Game.IsMouseVisible = false;
            }
            
            normalizedSin = 0.5 + Math.Sin(gameTime.TotalGameTime.TotalSeconds * Math.PI) * 0.5;
            
            var position = startPosition + (endPosition - startPosition) * (1 - TransitionPosition);
            titleDestination.X = (int) position.X;
            titleDestination.Y = (int) position.Y;

            subTitlePosition = subTitleStartPosition +
                               (subTitleEndPosition - subTitleStartPosition) * (1 - TransitionPosition);
            subTitlePosition.Y += (float)(normalizedSin - 0.5f) * 4;
        }

        public override void Draw(GameTime gameTime)
        {
            if (IsActive)
                ScreenManager.FadeBackBufferToColor(220, Color.Black);
            else 
                ScreenManager.FadeBackBufferToColor(TransitionAlpha * 220 / 255, Color.Black);

            var sb = ScreenManager.SpriteBatch;
            sb.Begin(blendState: BlendState.NonPremultiplied);

            Color drawColor = Color.White;
            drawColor.A = TransitionAlpha;
            sb.Draw(titleTexture, titleDestination, drawColor);
            
            drawColor = Color.OrangeRed;
            drawColor.A = TransitionAlpha;
            sb.DrawString(titleFont, SubTitleText, subTitlePosition, drawColor);
            
            drawColor = Color.Yellow;
            drawColor.A = (byte) (TransitionAlpha * normalizedSin);
            sb.DrawString(titleFont, StartGameText, startGameTextPos, drawColor);

            sb.End();
        }
    }
}