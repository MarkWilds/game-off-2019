using game.GameScreens;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game
{
    public class GameApplication : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        ScreenManager screenManager;

        public GameApplication()
        {
            graphics = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1024, PreferredBackBufferHeight = 768
            };
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            screenManager = new ScreenManager(spriteBatch, Content, GraphicsDevice, this);

            screenManager.ChangeScreen(new ShooterScreen());
        }

        protected override void Update(GameTime gameTime)
        {
            InputManager.Update();
            screenManager.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            screenManager.Draw(gameTime);
        }
    }
}