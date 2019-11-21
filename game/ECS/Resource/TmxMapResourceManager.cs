using System.Collections.Generic;
using System.IO;
using System.Linq;
using DefaultEcs;
using DefaultEcs.Resource;
using game.Data;
using game.ECS.Components;
using game.ECS.Events;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace game.ECS.Resource
{
    public class TmxMapResourceManager : AResourceManager<MapInfo, DisposableDummy<TmxMap>>
    {
        private readonly GraphicsDevice graphicsDevice;
        private readonly World ecsContext;
        private readonly string tilesetsPath;

        private readonly string mapPath;

        public TmxMapResourceManager(World context, GraphicsDevice graphicsDevice, string tilesetPath, string mapPath)
        {
            this.ecsContext = context;
            this.graphicsDevice = graphicsDevice;
            this.tilesetsPath = tilesetPath;
            this.mapPath = mapPath;
        }

        protected override DisposableDummy<TmxMap> Load(MapInfo info)
        {
            var relativePathToMapFile = Path.Combine(mapPath, $"{info.mapName}.tmx");
            return new DisposableDummy<TmxMap>(new TmxMap(relativePathToMapFile));
        }

        protected override void OnResourceLoaded(in Entity entity, MapInfo info, DisposableDummy<TmxMap> resource)
        {
            if(!entity.Has<Map>())
                entity.Set<Map>();

            ref var map = ref entity.Get<Map>();
            map.Data = resource.Data;
            map.Textures = new Dictionary<TmxTileset, Texture2D>();
            map.MapPropList = new List<MapProp>();
            map.PhysicsWorld = new Humper.World(map.Data.Width * map.Data.TileWidth, 
                map.Data.Height * map.Data.TileHeight);

            foreach (TmxTileset tileset in map.Data.Tilesets)
            {
                string pathToResource = Path.Combine(tilesetsPath, Path.GetFileName(tileset.Image.Source));
                if (!File.Exists(pathToResource))
                    continue;

                using (FileStream stream = new FileStream(pathToResource, FileMode.Open))
                {
                    Texture2D texture = Texture2D.FromStream(graphicsDevice, stream);
                    map.Textures.Add(tileset, texture);
                }
            }
            
            CreateColliders(map);
            CreateTriggers(map);
            
            ecsContext.Publish(new MapLoadedEvent(){entity = entity, startingSpawn = info.spawnName});
        }
        
        private void CreateColliders(Map map, string collisionLayer = "collision")
        {
            var data = map.Data;
            TmxObjectGroup objects = data.ObjectGroups[collisionLayer];

            foreach (var tmxObject in objects.Objects)
            {
                map.PhysicsWorld.Create((float)tmxObject.X, (float)tmxObject.Y,
                    (float)tmxObject.Width, (float)tmxObject.Height);
            }
        }

        private void CreateTriggers(Map map)
        {
            var data = map.Data;
            TmxObjectGroup objectGroup = data.ObjectGroups["objects"];
            
            foreach (var tmxObject in objectGroup.Objects
                .Where(o => o.Type == "trigger"))
            {
                var box = map.PhysicsWorld.Create((float)tmxObject.X, (float)tmxObject.Y,
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