using DefaultEcs.Resource;
using DefaultEcs.System;
using game.Components;
using game.Resource;
using game.Resource.Resources;
using game.Systems;
using Humper;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using World = DefaultEcs.World;

namespace game
{
    public class GameApplication : Game
    {
        private World ecsContext;
        private ISystem<double> ecsSystems;

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
                new MapRendererSystem(spriteBatch, blankTexture, ecsContext)
            );

            var mapEntity = ecsContext.CreateEntity();
            mapEntity.Set<Map>();
            mapEntity.Set(new ManagedResource<string, DisposableTmxMap>(@"Content/maps/test_fps.tmx"));

            var player = ecsContext.CreateEntity();
            player.Set<Transform2D>();
            player.Set(new Camera() {fov = 60.0f});
            player.Set(new Physics2D() {speed = 128});

            var tmxResourceManager = new TmxMapResourceManager(GraphicsDevice, @"Content/Tilesets");
            tmxResourceManager.Manage(ecsContext);

            // do some more setup after resources are loaded
            var map = mapEntity.Get<Map>();
            player.Set(map.physicsWorld.Create(40, 72, 
                map.Data.TileWidth / 2, map.Data.TileHeight / 2));

            // set player start position
            ref var transform = ref player.Get<Transform2D>();
            ref var collider = ref player.Get<IBox>();
            transform.position.X = collider.Bounds.Center.X;
            transform.position.Y = collider.Bounds.Center.Y;
        }

        protected override void Update(GameTime deltaTime)
        {
            ecsSystems.Update(deltaTime.ElapsedGameTime.TotalSeconds);
        }
    }
}