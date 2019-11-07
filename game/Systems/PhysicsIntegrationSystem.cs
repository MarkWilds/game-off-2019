using DefaultEcs;
using DefaultEcs.System;
using game.Components;

namespace game.Systems
{
    [With(typeof(Transform2D), typeof(Physics2D))]
    public class PhysicsIntegrationSystem : AEntitySystem<double>
    {
        public PhysicsIntegrationSystem(World world) : base(world)
        {
        }

        protected override void Update(double deltaTime, in Entity entity)
        {
            ref var transform = ref entity.Get<Transform2D>();
            var physics = entity.Get<Physics2D>();

            if (physics.direction.LengthSquared() > 0)
            {
                var velocity = physics.direction * physics.speed * (float) deltaTime;
                transform.position += velocity;
            }
        }
    }
}