using System;
using System.Collections.Generic;
using System.Linq;
using DefaultEcs;
using DefaultEcs.Resource;
using DefaultEcs.System;
using game.Data;
using game.ECS.Components;
using game.ECS.Events;
using game.ECS.Resource;
using game.ECS.Systems;
using game.Extensions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using TiledSharp;

namespace game.Screens
{
    public class SceneScreen : GameScreen
    {
        private Entity rootMapEntity;
        private ISystem<GameTime> updateSystems;
        private ISystem<GameTime> drawSystems;
        private ISystem<GameTime> playerControllerSystem;

        private const int VirtualScreenWidth = 320;
        private const int VirtualScreenHeight = 180;

        private const string MapsPathFolder = @"Content/maps";
        private const string TilesetsPathFolder = @"Content/Tilesets";

        private Dictionary<string, Song> songList;
        private MapInfo rootMapInfo;

        public SceneScreen(string startMap, string startSpawn)
        {
            songList = new Dictionary<string, Song>();
            rootMapInfo = new MapInfo() {mapName = startMap, spawnName = startSpawn};
        }
        
        public override void LoadContent()
        {
            var content = ScreenManager.Game.Content;
            var spriteBatch = ScreenManager.SpriteBatch;

            var ecsContext = ScreenManager.GlobalEcsContext;
            
            ecsContext.Subscribe(this);

            playerControllerSystem = new PlayerControllerSystem(ecsContext);
            updateSystems = playerControllerSystem;
            
            drawSystems = new RenderTargetRenderer(VirtualScreenWidth, VirtualScreenHeight, ScreenManager.Game.Window, spriteBatch,
                new SequentialSystem<GameTime>(
                    new SkyRendererSystem(ecsContext, spriteBatch, ScreenManager.BlankTexture, 
                        VirtualScreenWidth, VirtualScreenHeight),
                    new MapRendererSystem(ecsContext, VirtualScreenWidth, VirtualScreenHeight, spriteBatch),
                    new WeaponScreenRenderer(ecsContext, spriteBatch)
                )
            );

            // load resources
            new TmxMapResourceManager(ecsContext, ScreenManager.GraphicsDevice, TilesetsPathFolder,
                MapsPathFolder).Manage(ecsContext);
            new Texture2DResourceManager(content).Manage(ecsContext);

            // start loading map
            ScreenManager.Publish(new MapLoadEvent() {mapInfo = rootMapInfo});
        }

        public override void Draw(GameTime gameTime)
        {
            drawSystems.Update(gameTime);
        }

        public override void Update(GameTime gameTime)
        {
            if (IsActive)
            {
                if (Input.Input.IsKeyPressed(Keys.Escape))
                {
                    ScreenManager.Publish(new PlaySound(){soundName = "Sfx/Enter"});
                    ScreenManager.AddScreen(new PauseScreen());
                }
                
                updateSystems.Update(gameTime);
            }
        }

        [Subscribe]
        private void OnMapLoad(in MapLoadEvent @event)
        {
            rootMapEntity.Dispose();
            rootMapEntity = ScreenManager.GlobalEcsContext.CreateEntity();
            rootMapEntity.Set(new ManagedResource<MapInfo, DisposableDummy<TmxMap>>(@event.mapInfo));
        }

        [Subscribe]
        private void OnMapLoaded(in MapLoadedEvent @event)
        {
            var ecsContext = ScreenManager.GlobalEcsContext;
            var mapEntity = @event.entity;
            var map = mapEntity.Get<Map>();

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
                        mapEntity.Set(new ManagedResource<string[], DisposableDummy<Texture2D>>(
                            new []{@"Sprites/sky", @"Sprites/clouds"}));
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
    }
}