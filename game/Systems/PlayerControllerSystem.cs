using System;
using DefaultEcs;
using DefaultEcs.System;
using game.Components;
using Humper;
using Humper.Responses;
using Microsoft.Xna.Framework;
using World = DefaultEcs.World;

namespace game.Systems
{
    [With(typeof(Transform2D), typeof(Physics2D), typeof(Camera))]
    public class PlayerControllerSystem : AEntitySystem<double>
    {
        private const double Deadzone = 2.0;

        public PlayerControllerSystem(World world) : base(world)
        {
        }

        protected override void Update(double deltaTime, in Entity entity)
        {
            ref var transform = ref entity.Get<Transform2D>();
            ref var physics2D = ref entity.Get<Physics2D>();
            ref var collider = ref entity.Get<IBox>();
            
            var mouseDelta = InputManager.MouseAxisX;
            if (Math.Abs(mouseDelta) > Deadzone)
                transform.angle += mouseDelta * 20.0f * (float) deltaTime;

            double angleRad = transform.angle * Math.PI / 180;
            Vector2 forward = new Vector2((float) Math.Cos(angleRad),(float) Math.Sin(angleRad));
            Vector2 right = new Vector2(-forward.Y, forward.X);

            // basically a vector * matrix transformation
            var direction = forward * InputManager.VerticalAxis + right * InputManager.HorizontalAxis;
            if (direction.LengthSquared() > 0)
            {
                direction.Normalize();
                var velocity = direction * physics2D.speed * (float) deltaTime;

                collider.Move(collider.X + velocity.X, collider.Y + velocity.Y, 
                    collision => CollisionResponses.Slide);

                transform.position.X = collider.Bounds.Center.X;
                transform.position.Y = collider.Bounds.Center.Y;
            }
        }
    }
}