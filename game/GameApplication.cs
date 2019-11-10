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
        private ISystem<double> updateSystems;
        private ISystem<double> drawSystems;
        private Entity player;

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
            var spriteBatch = new SpriteBatch(GraphicsDevice);
            var blankTexture = Content.Load<Texture2D>("blank");

            ecsContext = new World(1337);
            ecsContext.Subscribe(this);
            
            updateSystems = new SequentialSystem<double>(
                new ActionSystem<double>(_ => InputManager.Update()),
                new PlayerControllerSystem(ecsContext)
            );

            drawSystems = new MapRendererSystem(spriteBatch, blankTexture, ecsContext);

            var mapEntity = ecsContext.CreateEntity();
            mapEntity.Set<Map>();
            mapEntity.Set(new ManagedResource<string, DisposableTmxMap>(@"Content/maps/test_fps.tmx"));

            player = ecsContext.CreateEntity();
            player.Set<Transform2D>();
            player.Set(new Camera() {fov = 60.0f});
            player.Set(new Physics2D() {speed = 128});

            var tmxResourceManager = new TmxMapResourceManager(ecsContext, GraphicsDevice,
                @"Content/Tilesets");
            tmxResourceManager.Manage(ecsContext);
        }

        protected override void Update(GameTime gameTime)
        {
            updateSystems.Update(gameTime.ElapsedGameTime.TotalSeconds);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            drawSystems.Update(gameTime.ElapsedGameTime.TotalSeconds);
        }

        [Subscribe]
        private void OnMapLoaded(in Map map)
        {
            var collider = map.physicsWorld.Create(40, 72,
                map.Data.TileWidth / 2, map.Data.TileHeight / 2);
            player.Set(collider);

            // set player start position
            ref var transform = ref player.Get<Transform2D>();
            transform.position.X = collider.Bounds.Center.X;
            transform.position.Y = collider.Bounds.Center.Y;
        }
    }
}