using System;
using DefaultEcs;
using DefaultEcs.Resource;
using game.Actions;
using game.ECS.Components;
using game.ECS.Events;
using game.ECS.Resource;
using game.Input.Virtual;
using game.StateMachine;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace game.Extensions
{
    public static class WorldExtensions
    {
        public static Entity CreateWeapon(this World world, int x, int y, int vMove, int hMove,
            params string[] textures)
        {
            var weapon = world.CreateEntity();
            weapon.Set<Texture2DResources>();
            weapon.Set(new ManagedResource<string[], DisposableDummy<Texture2D>>(textures));
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
            
            // use in statemachine
            var cameraEntity = world.GetEntities()
                .With(typeof(Transform2D))
                .With(typeof(Camera))
                .Build();

            var idleActionWeapon = new CallbackAction<float>();
            var swingActionWeapon = new CallbackAction<float>();
            var swingAction = new SequenceAction<float>(
                new TemporalAction(0.25f, swingActionWeapon),
                new TemporalAction(0.25f));

            idleActionWeapon.OnAct += f =>
            {
                var camera = cameraEntity.GetFirst();

                ref var cameraData = ref camera.Get<Camera>();
                ref var screenWeapon = ref weapon.Get<ScreenWeapon>();
                ref var texture2DDictionary = ref weapon.Get<Texture2DResources>();
                ref var transform = ref weapon.Get<Transform2D>();

                var texture = texture2DDictionary.textures[screenWeapon.resourceName];

                transform.position.X = screenWeapon.initialPosition.X -
                                       texture.Width / 2.0f + cameraData.bobFactor * screenWeapon.horizontalMoveFactor;
                transform.position.Y = screenWeapon.initialPosition.Y -
                                       texture.Height / 2.0f + cameraData.bobFactor * screenWeapon.verticalMoveFactor;

                return true;
            };

            swingActionWeapon.OnAct += t =>
            {
                ref var screenWeapon = ref weapon.Get<ScreenWeapon>();
                ref var transform = ref weapon.Get<Transform2D>();

                // add fast to slow tween
                transform.position.X = Easing.Back.InOut(t) * screenWeapon.initialPosition.X;
                transform.position.Y = screenWeapon.initialPosition.Y - Easing.Circular.Out(t) * 64;
                
                return true;
            };
            
            var weaponStateBuilder = new StateMachineBuilder();
            var weaponState = weaponStateBuilder.State("idle")
                .Enter(s =>
                {
                    ref var screenWeapon = ref weapon.Get<ScreenWeapon>();
                    screenWeapon.resourceName = "Sprites/blunt_weapon";
                })
                .Update((state, dt) => idleActionWeapon.Act(dt))
                .End()
                .State("swing_attack")
                .Enter(s =>
                {
                    ref var screenWeapon = ref weapon.Get<ScreenWeapon>();
                    screenWeapon.resourceName = "Sprites/blunt_swing_attack";
                    
                    world.Publish(new PlaySound(){soundName = "Sfx/Weapon/swish"});
                })
                .Update((state, dt) =>
                {
                    if (swingAction.Act(dt))
                    {
                        swingAction.Restart();
                        state.Parent.ChangeState("idle");
                    }
                })
                .End()
                .Build("idle");

            weapon.Set<IState>(weaponState);

            return weapon;
        }

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
    }
}