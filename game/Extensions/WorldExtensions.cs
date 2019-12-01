using DefaultEcs;
using DefaultEcs.Resource;
using game.ECS.Components;
using game.ECS.Resource;
using game.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.Extensions
{
    public static class WorldExtensions
    {
        public static Entity CreatePlayer(this World world, in Map map, int x, int y, int orientation)
        {
            var player = world.CreateEntity();
            player.Set<Transform2D>();
            player.Set(new Camera() {fov = 60.0f, pitch = 0, bobPeriod = 12f, bobAmplitude = 2f});
            player.Set(new Physics2D() {maxSpeed = 2, accelerationSpeed = 24});
            
            var collider = map.PhysicsWorld.Create(x, y,
                map.Data.TileWidth / 1.5f, map.Data.TileHeight / 1.5f);
            collider.AddTags(CollisionTag.Player);
            player.Set(collider);

            // set player data
            ref var transform = ref player.Get<Transform2D>();
            transform.position.X = collider.Bounds.Center.X;
            transform.position.Y = collider.Bounds.Center.Y;
            transform.orientation = orientation;

            return player;
        }
        
        public static Entity CreateWeapon(this World world, int x, int y, int vMove, int hMove)
        {
            var weapon = world.CreateEntity();
            weapon.Set<Transform2D>();
            weapon.Set(new ScreenWeapon()
            {
                horizontalMoveFactor = hMove,
                verticalMoveFactor = vMove,
                initialPosition = new Vector2(x, y)
            });
            weapon.Set(new Transform2D()
            {
                position = new Vector2(x, y)
            });
            
            return weapon;
        }
    }
}