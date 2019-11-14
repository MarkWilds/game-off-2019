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
        private const float MouseSpeed = 40.0f;
        private double bobTimer;
        private int bobSpeed = 18;
        private float bobOffset = 2f;
        
        private const float dragFactor = 0.1f;

        public PlayerControllerSystem(World world) : base(world)
        {
//            InputManager.CenterMouse = false;
        }

        protected override void Update(double deltaTime, in Entity entity)
        {
            ref var transform = ref entity.Get<Transform2D>();
            ref var camera = ref entity.Get<Camera>();
            ref var physics2D = ref entity.Get<Physics2D>();
            ref var collider = ref entity.Get<IBox>();
            
            bobTimer += deltaTime * bobSpeed;
            transform.angle += (float)(InputManager.MouseAxisX * MouseSpeed * deltaTime);

            double angleRad = transform.angle * Math.PI / 180;
            Vector2 forward = new Vector2((float) Math.Cos(angleRad),(float) Math.Sin(angleRad));
            Vector2 right = new Vector2(-forward.Y, forward.X);

            // force in the direction we want to move
            var accelerationDirection = forward * InputManager.VerticalAxis + right * InputManager.HorizontalAxis;
            var accumulatedForce = Vector2.Zero;
            if (accelerationDirection.LengthSquared() > 0)
            {
                accelerationDirection.Normalize();
                accumulatedForce += accelerationDirection * physics2D.accelerationSpeed;
            }

            // integration
            physics2D.velocity += accumulatedForce * (float) deltaTime;
            physics2D.velocity += physics2D.velocity * -dragFactor;

            // constraint
            var speed = physics2D.velocity.Length();
            if (speed > physics2D.maxSpeed)
            {
                physics2D.velocity.Normalize();
                physics2D.velocity *= physics2D.maxSpeed;
            }
            else if (speed <= 0.2f)
            {
                physics2D.velocity = Vector2.Zero;
            }

            var speedFactor = physics2D.velocity.Length() / physics2D.maxSpeed;
            camera.bobFactor = (int) (Math.Sin(bobTimer - Math.PI) * speedFactor * bobOffset);

            // do collision handling and resolve
            collider.Move(collider.X + physics2D.velocity.X, collider.Y + physics2D.velocity.Y, 
                collision => CollisionResponses.Slide);

            transform.position.X = collider.Bounds.Center.X;
            transform.position.Y = collider.Bounds.Center.Y;
        }
    }
}