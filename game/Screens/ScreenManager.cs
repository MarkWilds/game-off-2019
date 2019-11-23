using System;
using System.Collections.Generic;
using DefaultEcs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.Screens
{
    public class ScreenManager : DrawableGameComponent, IPublisher
    {
        private readonly List<GameScreen> screens;
        private readonly List<GameScreen> screensToUpdate;

        private readonly World globalEcsContext;

        private bool isInitialized;
        private SpriteBatch spriteBatch;
        private Texture2D blankTexture;

        public SpriteBatch SpriteBatch => spriteBatch;
        public World GlobalEcsContext => globalEcsContext;
        public Texture2D BlankTexture => blankTexture;

        public ScreenManager(Game game, int maxEntityCount)
            : base(game)
        {
            screens = new List<GameScreen>();
            screensToUpdate = new List<GameScreen>();
            globalEcsContext = new World(maxEntityCount);
        }
        
        public IDisposable Subscribe<T>(ActionIn<T> action)
        {
            return globalEcsContext.Subscribe<T>(action);
        }

        public void Publish<T>(in T message)
        {
            globalEcsContext.Publish(in message);
        }

        public override void Initialize()
        {
            base.Initialize();

            isInitialized = true;
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            blankTexture = new Texture2D(GraphicsDevice, 1,1);
            blankTexture.SetData(new[] {Color.White});

            foreach (GameScreen screen in screens)
            {
                screen.LoadContent();
            }
        }

        protected override void UnloadContent()
        {
            foreach (GameScreen screen in screens)
            {
                screen.UnloadContent();
            }
        }

        public override void Update(GameTime gameTime)
        {
            // Make a copy of the master screen list, to avoid confusion if
            // the process of updating one screen adds or removes others.
            screensToUpdate.Clear();

            foreach (GameScreen screen in screens)
                screensToUpdate.Add(screen);

            bool otherScreenHasFocus = !Game.IsActive;
            bool coveredByOtherScreen = false;

            // Loop as long as there are screens waiting to be updated.
            while (screensToUpdate.Count > 0)
            {
                // Pop the topmost screen off the waiting list.
                GameScreen screen = screensToUpdate[^1];

                screensToUpdate.RemoveAt(screensToUpdate.Count - 1);

                // Update the screen.
                screen.UpdateScreen(gameTime, otherScreenHasFocus, coveredByOtherScreen);

                if (screen.ScreenState == ScreenState.TransitionOn ||
                    screen.ScreenState == ScreenState.Active)
                {
                    if(!otherScreenHasFocus)
                        otherScreenHasFocus = true;

                    // If this is an active non-popup, inform any subsequent
                    // screens that they are covered by it.
                    if (!screen.IsPopup)
                        coveredByOtherScreen = true;
                }
            }
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (GameScreen screen in screens)
            {
                if (screen.ScreenState == ScreenState.Hidden)
                    continue;

                screen.Draw(gameTime);
            }
        }

        /// <summary>
        /// Adds a new screen to the screen manager.
        /// </summary>
        public void AddScreen(GameScreen screen)
        {
            screen.ScreenManager = this;
            screen.IsExiting = false;

            // If we have a graphics device, tell the screen to load content.
            if (isInitialized)
            {
                screen.LoadContent();
            }

            screens.Add(screen);
        }

        /// <summary>
        /// Removes a screen from the screen manager. You should normally
        /// use GameScreen.ExitScreen instead of calling this directly, so
        /// the screen can gradually transition off rather than just being
        /// instantly removed.
        /// </summary>
        public void RemoveScreen(GameScreen screen)
        {
            // If we have a graphics device, tell the screen to unload content.
            if (isInitialized)
            {
                screen.UnloadContent();
            }

            screens.Remove(screen);
            screensToUpdate.Remove(screen);
        }

        /// <summary>
        /// Helper draws a translucent black fullscreen sprite, used for fading
        /// screens in and out, and for darkening the background behind popups.
        /// </summary>
        public void FadeBackBufferToColor(int alpha, in Color col)
        {
            spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

            Color color = col;
            color.A = (byte) alpha;

            Rectangle destination = default;
            destination.Width = GraphicsDevice.Viewport.Width;
            destination.Height = GraphicsDevice.Viewport.Height;

            spriteBatch.Draw(blankTexture, destination, color);

            spriteBatch.End();
        }
    }
}