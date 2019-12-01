using DefaultEcs;
using DefaultEcs.Resource;
using game.Actions;
using game.ECS.Components;
using game.ECS.Events;
using game.ECS.Resource;
using game.Extensions;
using game.StateMachine;
using Microsoft.Xna.Framework.Graphics;

namespace game.Weapons
{
    public class Hammer : IWeapon
    {
        private World world;
        private Entity weapon;
        private EntitySet cameraEntitySet;

        private CallbackAction<float> swingAttackAction;
        private TemporalAction temporalSwingAction;
        private IAction<float> swingAction;

        private IState weaponState;
        
        private readonly string[] textures =
            {"Sprites/blunt_weapon", "Sprites/blunt_swing_attack"};

        private readonly string weaponWooshSound = "Sfx/Weapon/swish";

        public enum WeaponState
        {
            IDLE,
            SWING
        }

        public void Initialize(World ecsContent, in Entity weaponEnt)
        {
            world = ecsContent;
            weapon = weaponEnt;
            
            weapon.Set<Texture2DResources>();
            weapon.Set(new ManagedResource<string[], DisposableDummy<Texture2D>>(textures));

            cameraEntitySet = world.GetEntities()
                .With(typeof(Transform2D))
                .With(typeof(Camera))
                .Build();

            swingAttackAction = new CallbackAction<float>();
            swingAttackAction.OnAct += DoSwingActionUpdate;
            
            temporalSwingAction = new TemporalAction(0.25f, swingAttackAction);
            swingAction = new SequenceAction<float>(temporalSwingAction,
                new TemporalAction(0.25f));

            var weaponStateBuilder = new StateMachineBuilder();
            weaponState = weaponStateBuilder
                .State(WeaponState.IDLE.ToString())
                .Enter(s =>EnterIdleWeapon())
                .Update((state, dt) => UpdateIdleWeapon())
                .End()
                .State(WeaponState.SWING.ToString())
                .Enter(s => EnterSwingAttack())
                .Update((state, dt) => UpdateSwingAttack(dt))
                .End()
                .Build(WeaponState.IDLE.ToString());
            
            weapon.Set(weaponState);
        }

        private void EnterIdleWeapon()
        {
            ref var screenWeapon = ref weapon.Get<ScreenWeapon>();
            screenWeapon.resourceName = textures[0];
        }

        private void UpdateIdleWeapon()
        {
            var camera = cameraEntitySet.GetFirst();

            ref var cameraData = ref camera.Get<Camera>();
            ref var screenWeapon = ref weapon.Get<ScreenWeapon>();
            ref var texture2DDictionary = ref weapon.Get<Texture2DResources>();
            ref var transform = ref weapon.Get<Transform2D>();

            var texture = texture2DDictionary.textures[screenWeapon.resourceName];

            transform.position.X = screenWeapon.initialPosition.X -
                                   texture.Width / 2.0f + cameraData.bobFactor * screenWeapon.horizontalMoveFactor;
            transform.position.Y = screenWeapon.initialPosition.Y -
                                   texture.Height / 2.0f + cameraData.bobFactor * screenWeapon.verticalMoveFactor;
        }

        private void EnterSwingAttack()
        {
            ref var screenWeapon = ref weapon.Get<ScreenWeapon>();
            screenWeapon.resourceName = textures[1];

            temporalSwingAction.Reverse = !temporalSwingAction.Reverse;
            world.Publish(new PlaySound(){soundName = weaponWooshSound});
        }

        private void UpdateSwingAttack(float dt)
        {
            if (swingAction.Act(dt))
            {
                swingAction.Restart();
                weaponState.ChangeState(WeaponState.IDLE.ToString());
            }
        }

        private bool DoSwingActionUpdate(float t)
        {
            ref var screenWeapon = ref weapon.Get<ScreenWeapon>();
            ref var transform = ref weapon.Get<Transform2D>();

            transform.position.X = Easing.Back.InOut(t) * screenWeapon.initialPosition.X;

            var f = temporalSwingAction.Reverse ? 1 - t : t;
            transform.position.Y = screenWeapon.initialPosition.Y - Easing.Circular.Out(f) * 64;
                
            return true;
        }
    }
}