﻿using DefaultEcs;
using DefaultEcs.Resource;
using game.ECS.Components;
using game.ECS.Resource;
using Microsoft.Xna.Framework.Graphics;

namespace game.Extensions
{
    public static class WorldExtensions
    {
        public static Entity CreateWeapon(this World world, int x, int y, string resourceName)
        {
            var weapon = world.CreateEntity();
            weapon.Set<Texture2DResources>();
            weapon.Set(new ManagedResource<string, DisposableDummy<Texture2D>>(resourceName));
            weapon.Set<Transform2D>();
            weapon.Set( new ScreenWeapon()
            {
                resourceName = resourceName,
                horizontalMoveFactor = 3,
                verticalMoveFactor = 1
            });
            
            ref var transform = ref weapon.Get<Transform2D>();
            transform.position.X = x;
            transform.position.Y = y;

            return weapon;
        }

        public static Entity CreatePlayer(this World world, in Map map, int x, int y, int orientation)
        {
            var player = world.CreateEntity();
            player.Set<Transform2D>();
            player.Set(new Camera() {fov = 60.0f});
            player.Set(new Physics2D() {maxSpeed = 2, accelerationSpeed = 24});
            
            var collider = map.physicsWorld.Create(x, y,
                map.Data.TileWidth / 2.0f, map.Data.TileHeight / 2.0f);
            player.Set(collider);

            // set player data
            ref var transform = ref player.Get<Transform2D>();
            transform.position.X = collider.Bounds.Center.X;
            transform.position.Y = collider.Bounds.Center.Y;
            transform.orientation = orientation;

            return player;
        }
    }
}