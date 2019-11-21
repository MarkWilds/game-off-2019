using game.Screens;
using Microsoft.Xna.Framework;

namespace game
{
    public class GameApplication : Game
    {
        static void Main()
        {
            using var game = new GameApplication();
            game.Run();
        }

        private GameApplication()
        {
            IsFixedTimeStep = true;
            var manager = new GraphicsDeviceManager(this);
            manager.PreferredBackBufferWidth = 1280;
            manager.PreferredBackBufferHeight = 720;
            
            Content.RootDirectory = "Content";
            Window.Title = "Whack a Monster!!!";
            
            var screenManager = new ScreenManager(this, 1 << 8);
            Components.Add(screenManager);
            
            screenManager.AddScreen( new SceneScreen());
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Input.Update(gameTime);
            base.Update(gameTime);
        }
    }
}