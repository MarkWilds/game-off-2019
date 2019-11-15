using System;
using DefaultEcs;
using DefaultEcs.System;
using game.ECS.Components;
using game.Input.Virtual;
using Humper;
using Humper.Responses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using World = DefaultEcs.World;

namespace game.ECS.Systems
{
    [With(typeof(Transform2D), typeof(Physics2D), typeof(Camera))]
    public class PlayerControllerSystem : AEntitySystem<GameTime>
    {
        private const float MouseSpeed = 20.0f;
        private double bobTimer;
        private int bobSpeed = 12;
        private float bobOffset = 2f;
        
        private const float dragFactor = 0.1f;

        private readonly VirtualIntegerAxis horizontalAxis;
        private readonly VirtualIntegerAxis verticalAxis;

        public PlayerControllerSystem(World world) : base(world)
        {
            horizontalAxis = new VirtualIntegerAxis();
            horizontalAxis.AddKeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.A, Keys.D);
            
            verticalAxis = new VirtualIntegerAxis();
            verticalAxis.AddKeyboardKeys(VirtualInput.OverlapBehavior.CancelOut, Keys.S, Keys.W);
        }

        protected override void Update(GameTime time, in Entity entity)
        {
            ref var transform = ref entity.Get<Transform2D>();
            ref var camera = ref entity.Get<Camera>();
            ref var physics2D = ref entity.Get<Physics2D>();
            ref var collider = ref entity.Get<IBox>();
            
            bobTimer += time.ElapsedGameTime.TotalSeconds * bobSpeed;
            var mouseDelta = Input.Input.MousePositionDelta;
            transform.orientation += (float)(mouseDelta.X * MouseSpeed * time.ElapsedGameTime.TotalSeconds);

            double angleRad = transform.orientation * Math.PI / 180;
            Vector2 forward = new Vector2((float) Math.Cos(angleRad),(float) Math.Sin(angleRad));
            Vector2 right = new Vector2(-forward.Y, forward.X);

            // force in the direction we want to move
            var accelerationDirection = forward * verticalAxis + right * horizontalAxis;
            var accumulatedForce = Vector2.Zero;
            if (accelerationDirection.LengthSquared() > 0)
            {
                accelerationDirection.Normalize();
                accumulatedForce += accelerationDirection * physics2D.accelerationSpeed;
            }

            // integration
            physics2D.velocity += accumulatedForce * (float) time.ElapsedGameTime.TotalSeconds;
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