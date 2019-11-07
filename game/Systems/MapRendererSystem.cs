using DefaultEcs;
using DefaultEcs.System;
using game.Components;
using Microsoft.Xna.Framework.Graphics;

namespace game.Systems
{
    [With(typeof(MapData))]
    public class MapRendererSystem : AEntitySystem<double>
    {
        private readonly SpriteBatch spriteBatch;
        private readonly RaycastRenderer renderer;

        private readonly EntitySet cameraBuilder;
        private Entity camera;
        
        public MapRendererSystem(SpriteBatch batch, 
            Texture2D blankTexture, World world)
        : base(world)
        {
            cameraBuilder = world.GetEntities()
                .With(typeof(Transform2D))
                .With(typeof(CameraData))
                .Build();
                        
            spriteBatch = batch;
            renderer = new RaycastRenderer(batch.GraphicsDevice.Viewport, blankTexture);
        }

        protected override void PreUpdate(double state)
        {
            camera = cameraBuilder.GetEntities()[0];
            spriteBatch.Begin();
        }

        protected override void Update(double state, in Entity entity)
        {
            var mapData = entity.Get<MapData>();
            var cameraTransform = camera.Get<Transform2D>();
            var cameraData = camera.Get<CameraData>();
            
            renderer.ClearDepthBuffer();
            renderer.RenderMap(spriteBatch, mapData, cameraTransform.position, cameraTransform.angle,
                32, "walls1", cameraData.fov);

//            RenderProps(spriteBatch);
        }

        protected override void PostUpdate(double state)
        {
            spriteBatch.End();
        }
        
//        private void RenderProps(SpriteBatch batch)
//        {
//            TmxLayer propsLayer = currentMap.Data.Layers["props"];
//            List<TmxLayerTile> propTiles = propsLayer.Tiles.Where(t => t.Gid > 0).ToList();
//
//            foreach (TmxLayerTile propTile in propTiles)
//            {
//                TmxTileset tileset = currentMap.GetTilesetForTile(propTile);
//                if (tileset == null)
//                    continue;
//
//                int halfCellSize = cellSize / 2;
//                Texture2D propTexture = currentMap.Textures[tileset];
//                Vector2 spritePosition = new Vector2(propTile.X * cellSize + halfCellSize,
//                    propTile.Y * cellSize + halfCellSize);
//                Rectangle source = currentMap.GetSourceRectangleForTile(tileset, propTile);
//
//                renderer.RenderSprite(batch, spritePosition, propTexture, source,
//                    position, angle);
//            }
//        }
    }
}