using DefaultEcs;
using DefaultEcs.Resource;
using DefaultEcs.System;
using game.Components;
using game.Resource;
using game.Resource.Resources;
using game.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game
{
    public class GameApplication : Game
    {
        private World ecsContext;
        private ISystem<double> ecsSystems;

        private AResourceManager<string, DisposableTmxMap> tmxResourceManager;

        public GameApplication()
        {
            IsFixedTimeStep = true;
            new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1024, PreferredBackBufferHeight = 768
            };
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
            Texture2D blankTexture = Content.Load<Texture2D>("blank");
            
            ecsContext = new World(1337);
            ecsSystems = new SequentialSystem<double>(
                new ActionSystem<double>(_ => InputManager.Update()),
                new PlayerControllerSystem(ecsContext),
                new PhysicsIntegrationSystem(ecsContext),
                new MapRendererSystem(spriteBatch, blankTexture, ecsContext)
            );
            
            var mapEntity = ecsContext.CreateEntity();
            mapEntity.Set<Map>();
            mapEntity.Set(new ManagedResource<string, DisposableTmxMap>(@"Content/maps/test_fps.tmx"));

            var camera = ecsContext.CreateEntity();
            camera.Set(ecsContext.CreateTransform2D(32 + 16,64 + 16, 0));
            camera.Set(new Camera(){fov = 60.0f});
            camera.Set(new Physics2D()
            {
                speed = 128, 
                direction = default
            });

            tmxResourceManager = new TmxMapResourceManager(GraphicsDevice, @"Content/Tilesets");
            tmxResourceManager.Manage(ecsContext);
        }

        protected override void Update(GameTime deltaTime)
        {
            ecsSystems.Update(deltaTime.ElapsedGameTime.TotalSeconds);
        }
    }
}