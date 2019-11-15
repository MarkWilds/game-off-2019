using System;
using System.Linq;
using DefaultEcs;
using DefaultEcs.Resource;
using DefaultEcs.System;
using game.ECS.Components;
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

            var preferedSourceRectangle = GetPreferedScreenSizeRectangle(320, 240);
            drawSystems =  new RenderTargetRenderer(320, 240, preferedSourceRectangle, spriteBatch,
                new SequentialSystem<GameTime>(
                    new SkyRendererSystem(ecsContext, spriteBatch, 320, 240), 
                    new MapRendererSystem(320, 240, spriteBatch, ecsContext)
                    )
                );
            
            new TmxMapResourceManager(ecsContext, GraphicsDevice,
                @"Content/Tilesets").Manage(ecsContext);
            new Texture2DResourceManager(Content).Manage(ecsContext);

            var mapEntity = ecsContext.CreateEntity();
            mapEntity.Set<Map>();
            mapEntity.Set<Texture2DResources>();
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
        private void OnMapLoaded(in Entity mapEntity)
        {
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

        private Rectangle GetPreferedScreenSizeRectangle(int width, int height)
        {
            var windowWidth = Window.ClientBounds.Width;
            var windowHeight = Window.ClientBounds.Height;
            float outputAspect = windowWidth / (float)windowHeight;
            float preferredAspect = width / (float)height;
            
            if (outputAspect <= preferredAspect)
            {
                // output is taller than it is wider, bars on top/bottom
                int presentHeight = (int)((windowWidth / preferredAspect) + 0.5f);
                int barHeight = (windowHeight - presentHeight) / 2;
                return new Rectangle(0, barHeight, windowWidth, presentHeight);
            }

            // output is wider than it is tall, bars left/right
            int presentWidth = (int)((windowHeight * preferredAspect) + 0.5f);
            int barWidth = (windowWidth - presentWidth) / 2;
            return new Rectangle(barWidth, 0, presentWidth, windowHeight);
        }
    }
}