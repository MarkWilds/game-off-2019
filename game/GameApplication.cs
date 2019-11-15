using System;
using System.Linq;
using DefaultEcs;
using DefaultEcs.Resource;
using DefaultEcs.System;
using game.ECS.Components;
using game.ECS.Events;
using game.ECS.Resource;
using game.ECS.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace game
{
    public class GameApplication : Game
    {
        private World ecsContext;
        private ISystem<GameTime> updateSystems;
        private ISystem<GameTime> drawSystems;
        private const int VirtualScreenWidth = 320;
        private const int VirtualScreenHeight = 240;

        public GameApplication()
        {
            IsFixedTimeStep = true;
            new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1024, PreferredBackBufferHeight = 768,
//                IsFullScreen = true
            };
            Content.RootDirectory = "Content";
        }

        protected override void LoadContent()
        {
            var spriteBatch = new SpriteBatch(GraphicsDevice);

            ecsContext = new World(1 << 8);
            ecsContext.Subscribe(this);
            
            updateSystems = new SequentialSystem<GameTime>(
                new ActionSystem<GameTime>(Input.Input.Update),
                new PlayerControllerSystem(ecsContext)
            );
            
            drawSystems =  new RenderTargetRenderer(VirtualScreenWidth, VirtualScreenHeight, Window, spriteBatch,
                new SequentialSystem<GameTime>(
                    new SkyRendererSystem(ecsContext, spriteBatch, VirtualScreenWidth, VirtualScreenHeight), 
                    new MapRendererSystem(VirtualScreenWidth, VirtualScreenHeight, spriteBatch, ecsContext)
                    )
                );
            
            new TmxMapResourceManager(ecsContext, GraphicsDevice,
                @"Content/Tilesets").Manage(ecsContext);
            new Texture2DResourceManager(Content).Manage(ecsContext);

            var mapEntity = ecsContext.CreateEntity();
            mapEntity.Set(new ManagedResource<string, DisposableTmxMap>(@"Content/maps/test_fps.tmx"));
        }

        protected override void Update(GameTime gameTime)
        {
            updateSystems.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            drawSystems.Update(gameTime);
        }

        [Subscribe]
        private void OnMapLoaded(in MapLoadedEvent @event)
        {
            var mapEntity = @event.entity;
            var map = mapEntity.Get<Map>();
            var darknessFactor = Int32.Parse(map.Data.Properties["darknessFactor"]);
            
            mapEntity.Set(new ManagedResource<string[], Texture2D>(new []{@"Sprites/sky", @"Sprites/clouds"}));
            mapEntity.Set(new MapRenderData(){darknessFactor = darknessFactor});

            var objects = map.Data.ObjectGroups["objects"];
            TmxObject spawn = objects.Objects
                .Where(o => o.Type == "spawn")
                .SingleOrDefault(o => o.Name == "spawn01");

            // create player
            var player = ecsContext.CreateEntity();
            player.Set<Transform2D>();
            player.Set(new Camera() {fov = 60.0f});
            player.Set(new Physics2D() {maxSpeed = 2, accelerationSpeed = 24});
            
            var collider = map.physicsWorld.Create((float) spawn.X, (float) spawn.Y,
                map.Data.TileWidth / 2.0f, map.Data.TileHeight / 2.0f);
            player.Set(collider);

            // set player data
            ref var transform = ref player.Get<Transform2D>();
            transform.position.X = collider.Bounds.Center.X;
            transform.position.Y = collider.Bounds.Center.Y;
            transform.orientation = Int32.Parse(spawn.Properties["orientation"]);
        }
    }
}