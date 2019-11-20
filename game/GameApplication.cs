using System;
using System.Collections.Generic;
using System.Linq;
using DefaultEcs;
using DefaultEcs.Resource;
using DefaultEcs.System;
using game.ECS.Components;
using game.ECS.Events;
using game.ECS.Resource;
using game.ECS.Systems;
using game.Extensions;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TiledSharp;
using Vector2 = System.Numerics.Vector2;

namespace game
{
    public class GameApplication : Game
    {
        private World ecsContext;
        private Entity currentMapEntity;
        private ISystem<GameTime> updateSystems;
        private ISystem<GameTime> drawSystems;

        private const int VirtualScreenWidth = 320;
        private const int VirtualScreenHeight = 180;

        private const string MapsPathFolder = @"Content/maps";
        private const string TilesetsPathFolder = @"Content/Tilesets";
        private const string StartingMapName = @"hub";
        private const string StartingSpawnName = @"spawn01";

        private Dictionary<string, Song> songList;

        // for debugging
        private ImGuiRenderer imguiRenderer;
        private ISystem<GameTime> playerControllerSystem;
        private bool debug;
        private EntitySet entitySet;

        static void Main()
        {
            using var game = new GameApplication();
            game.Run();
        }

        public GameApplication()
        {
            IsFixedTimeStep = true;
            new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1280, PreferredBackBufferHeight = 720,
            };
            Content.RootDirectory = "Content";
            Window.Title = "Whack a Monster!!!";

            songList = new Dictionary<string, Song>();
        }

        protected override void LoadContent()
        {
            imguiRenderer = new ImGuiRenderer(this);
            imguiRenderer.RebuildFontAtlas();
            var spriteBatch = new SpriteBatch(GraphicsDevice);

            ecsContext = new World(1 << 8);
            ecsContext.Subscribe(this);

            playerControllerSystem = new PlayerControllerSystem(ecsContext);
            updateSystems = new SequentialSystem<GameTime>(
                new ActionSystem<GameTime>(Input.Input.Update),
                playerControllerSystem
            );

            drawSystems = new RenderTargetRenderer(VirtualScreenWidth, VirtualScreenHeight, Window, spriteBatch,
                new SequentialSystem<GameTime>(
                    new SkyRendererSystem(ecsContext, spriteBatch, VirtualScreenWidth, VirtualScreenHeight),
                    new MapRendererSystem(ecsContext, VirtualScreenWidth, VirtualScreenHeight, spriteBatch),
                    new WeaponScreenRenderer(ecsContext, spriteBatch)
                )
            );

            // load resources
            songList.Add("Music/hub", Content.Load<Song>("Music/hub"));
            songList.Add("Music/dungeon01", Content.Load<Song>("Music/dungeon01"));
            songList.Add("Music/whackmole", Content.Load<Song>("Music/whackmole"));

            new TmxMapResourceManager(ecsContext, GraphicsDevice, TilesetsPathFolder,
                MapsPathFolder).Manage(ecsContext);
            new Texture2DResourceManager(Content).Manage(ecsContext);

            // start loading map
            var mapInfo = new MapInfo() {mapName = StartingMapName, spawnName = StartingSpawnName};
            ecsContext.Publish(new MapLoadEvent() {mapInfo = mapInfo});
            
            // debug
            entitySet = ecsContext.GetEntities().WithEither<ScreenWeapon, Physics2D, Camera>().Build();
        }

        private void CheckDebugGUI()
        {
            if (Input.Input.IsKeyPressed(Keys.Tab))
            {
                playerControllerSystem.IsEnabled = !playerControllerSystem.IsEnabled;
                IsMouseVisible = !playerControllerSystem.IsEnabled;
                debug = !playerControllerSystem.IsEnabled;
            }
        }

        protected override void Update(GameTime gameTime)
        {
            updateSystems.Update(gameTime);

            CheckDebugGUI();
        }

        private void DoGui(GameTime gameTime)
        {
            imguiRenderer.BeforeLayout(gameTime);

            ImGuiMainMenuBar(@event =>
            {
                if (@event == "quit")
                    Exit();
            });
            
            ImGui.SetWindowSize(new Vector2(400, 300));
            ImGui.Begin("DEBUG");
            
            foreach (ref readonly var entity in entitySet.GetEntities())
            {
                if (entity.Has<Camera>())
                {
                    ref var data = ref entity.Get<Camera>();
                    
                    ImGui.Text("Camera:");
                    ImGui.SliderInt("Up/Down head rotation", ref data.pitch, -64, 64);
                    ImGui.SliderFloat("Field of view", ref data.fov, 30, 90);
                    ImGui.SliderFloat("Bobbing speed", ref data.bobPeriod, 1, 64);
                    ImGui.SliderFloat("Bobbing height", ref data.bobAmplitude, 1, 64);

                    if (ImGui.Button("Default##1"))
                    {
                        data.fov = 60f;
                        data.pitch = 0;
                        data.bobPeriod = 12;
                        data.bobAmplitude = 2;
                    }
                }

                if (entity.Has<ScreenWeapon>())
                {
                    ref var data = ref entity.Get<ScreenWeapon>();
                    ImGui.Text("Weapon:");
                    ImGui.SliderFloat("Horizontal move factor", ref data.horizontalMoveFactor, 1, 100);
                    ImGui.SliderFloat("Vertical move factor", ref data.verticalMoveFactor, 1, 100);
                    
                    if (ImGui.Button("Default##2"))
                    {
                        data.horizontalMoveFactor = 2;
                        data.verticalMoveFactor = 1;
                    }
                }
                
                if (entity.Has<Physics2D>())
                {
                    ref var data = ref entity.Get<Physics2D>();
                    ImGui.Text("Physics:");
                    ImGui.SliderFloat("Acceleration speed", ref data.accelerationSpeed, 1, 100);
                    ImGui.SliderFloat("Max Speed", ref data.maxSpeed, 1, 100);
                    
                    if (ImGui.Button("Default##3"))
                    {
                        data.maxSpeed = 2;
                        data.accelerationSpeed = 24;
                    }
                }
            }
            
            ImGui.Text("Maak maar screenshot als het goed zit");
            ImGui.End();

            
            imguiRenderer.AfterLayout();
        }
        
        private void ImGuiMainMenuBar(Action<string> events)
        {
            if (ImGui.BeginMainMenuBar())
            {
                if (ImGui.BeginMenu("File"))
                {
                    if (ImGui.MenuItem("Quit", "ALT+F4"))
                        events("quit");

                    ImGui.EndMenu();
                }

                ImGui.EndMainMenuBar();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            drawSystems.Update(gameTime);
            
            if(debug)
                DoGui(gameTime);
        }

        [Subscribe]
        private void OnStopSong(in StopSongEvent @event)
        {
            MediaPlayer.Stop();
        }

        [Subscribe]
        private void OnPlaySong(in PlaySongEvent @event)
        {
            var song = songList[@event.songName];
            MediaPlayer.IsRepeating = @event.isRepeating;
            MediaPlayer.Play(song);
        }

        [Subscribe]
        private void OnMapLoad(in MapLoadEvent @event)
        {
            currentMapEntity.Dispose();
            currentMapEntity = ecsContext.CreateEntity();
            currentMapEntity.Set(new ManagedResource<MapInfo, DisposableDummy<TmxMap>>(@event.mapInfo));
        }

        [Subscribe]
        private void OnMapLoaded(in MapLoadedEvent @event)
        {
            var mapEntity = @event.entity;
            var map = mapEntity.Get<Map>();

            CreateColliders(map);
            CreateTriggers(map);

            var darknessFactor = 300;
            if (map.Data.Properties.ContainsKey("darknessFactor"))
                darknessFactor = Int32.Parse(map.Data.Properties["darknessFactor"]);
            var mapRenderData = new MapRenderData() {darknessFactor = darknessFactor};

            if (map.Data.Properties.ContainsKey("sky"))
            {
                var skyProperty = map.Data.Properties["sky"];
                mapRenderData.skyType = (SkyType) Enum.Parse(typeof(SkyType), skyProperty);
                switch (skyProperty)
                {
                    case "clouds":
                        mapEntity.Set<Texture2DResources>();
                        mapEntity.Set(new ManagedResource<string[], DisposableDummy<Texture2D>>(new []{@"Sprites/sky", @"Sprites/clouds"}));
                        break;
                    case "solid":
                        var skyColor = map.Data.Properties["skyColor"];
                        mapRenderData.skyColor = ColorExtensions.FromRgb(skyColor);
                        break;
                }
            }

            mapEntity.Set(in mapRenderData);

            var objects = map.Data.ObjectGroups["objects"];
            var spawnName = @event.startingSpawn;
            TmxObject spawn = objects.Objects
                .Where(o => o.Type == "spawn")
                .SingleOrDefault(o => o.Name == spawnName);

            var playerOrientation = Int32.Parse(spawn.Properties["orientation"]);

            Entity player = ecsContext.CreatePlayer(in map, (int) spawn.X, (int) spawn.Y, playerOrientation);
            mapEntity.SetAsParentOf(in player);

            Entity weapon = ecsContext.CreateWeapon(VirtualScreenWidth / 2 + 64, VirtualScreenHeight - 16,
                1, 2, "Sprites/blunt_weapon");
            player.SetAsParentOf(in weapon);

            if (map.Data.Properties.ContainsKey("music"))
            {
                var mapMusic = map.Data.Properties["music"];
                PlaySongEvent songEvent = default;
                songEvent.songName = mapMusic;
                songEvent.isRepeating = true;
                ecsContext.Publish(songEvent);
            }
            else
            {
                ecsContext.Publish(new StopSongEvent());
            }
        }

        private void CreateColliders(Map map, string collisionLayer = "collision")
        {
            var data = map.Data;
            TmxObjectGroup objects = data.ObjectGroups[collisionLayer];

            foreach (var tmxObject in objects.Objects)
            {
                map.physicsWorld.Create((float)tmxObject.X, (float)tmxObject.Y,
                    (float)tmxObject.Width, (float)tmxObject.Height);
            }
        }

        private void CreateTriggers(Map map)
        {
            var data = map.Data;
            TmxObjectGroup objects = data.ObjectGroups["objects"];
            
            foreach (var tmxObject in objects.Objects
                .Where(o => o.Type == "trigger"))
            {
                var box = map.physicsWorld.Create((float)tmxObject.X, (float)tmxObject.Y,
                    (float)tmxObject.Width, (float)tmxObject.Height);
                
                var triggerType = tmxObject.Properties["type"];

                if (triggerType == "teleport")
                {
                    var teleportToMap = tmxObject.Properties["map"];
                    var toSpawn = tmxObject.Properties["spawn"];

                    box.Data = new TriggerInfo(){type = triggerType, data = new { map = teleportToMap, spawn = toSpawn}};
                }
            }
        }
    }
}