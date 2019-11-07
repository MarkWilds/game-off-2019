using System.Collections.Generic;
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
                new MapRendererSystem(spriteBatch, blankTexture, ecsContext)
            );
            
            var mapEntity = ecsContext.CreateEntity();
            mapEntity.Set<MapData>();
            mapEntity.Set(new ManagedResource<string, DisposableTmxMap>(@"Content/maps/test_fps.tmx"));

            var camera = ecsContext.CreateEntity();
            camera.Set(ecsContext.CreateTransform2D(32 + 16,64 + 16, 0));
            camera.Set(new CameraData(){fov = 60.0f});
            camera.Set(new PhysicsBodyData(){speed = 128});

            tmxResourceManager = new TmxMapResourceManager(GraphicsDevice, @"Content/Tilesets");
            tmxResourceManager.Manage(ecsContext);
        }

        protected override void Update(GameTime deltaTime)
        {
            ecsSystems.Update(deltaTime.ElapsedGameTime.TotalSeconds);
            
//            var mouseDelta = InputManager.MouseAxisX;
//            var deadzone = 2.0;
//            if(Math.Abs(mouseDelta) > deadzone)
//                angle += mouseDelta * 20.0f * (float) deltaTime.ElapsedGameTime.TotalSeconds;
//
//            double angleRad = angle * Math.PI / 180;
//            Vector2 forward = new Vector2((float) Math.Cos(angleRad),(float) Math.Sin(angleRad));
//            Vector2 right = new Vector2(-forward.Y, forward.X);
//
//            // basically a vector * matrix transformation
//            Vector2 movementDirection = forward * InputManager.VerticalAxis + right * InputManager.HorizontalAxis;
//            if (movementDirection.LengthSquared() > 0)
//            {
//                movementDirection.Normalize();
//
//                Vector2 velocity = movementDirection * movementSpeed * (float) deltaTime.ElapsedGameTime.TotalSeconds;
//
//                // do collision detection
//                RayCaster.HitData hitData;
//                float dirAngle = (float) Math.Atan2(velocity.Y, velocity.X);
//                if (RayCaster.RayIntersectsGrid(position, dirAngle, 32, out hitData,
//                    currentMap.GetIsTileOccupiedFunction("walls1"), 16))
//                {
//                    if (hitData.rayLength >= 0)
//                        velocity.Normalize();
//                }
//                
////                velocity = currentMap.Move(velocity, position);
//                position += velocity;
//            }
        }
    }
}