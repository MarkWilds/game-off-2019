using System;
using DefaultEcs;
using DefaultEcs.System;
using game.Components;
using Microsoft.Xna.Framework;

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
            
            var mouseDelta = InputManager.MouseAxisX;
            if (Math.Abs(mouseDelta) > Deadzone)
                transform.angle += mouseDelta * 20.0f * (float) deltaTime;

            double angleRad = transform.angle * Math.PI / 180;
            Vector2 forward = new Vector2((float) Math.Cos(angleRad),(float) Math.Sin(angleRad));
            Vector2 right = new Vector2(-forward.Y, forward.X);

            // basically a vector * matrix transformation
            physics2D.direction = forward * InputManager.VerticalAxis + right * InputManager.HorizontalAxis;
//            if (movementDirection.LengthSquared() > 0)
//            {
//                movementDirection.Normalize();
//
//                Vector2 velocity = movementDirection * movementSpeed * (float) deltaTime.ElapsedGameTime.TotalSeconds;
//
//                // do collision detection
//                RayCaster.HitData hitData;
//                float dirAngle = (float) Math.Atan2(velocity.Y, velocity.X);
//                if (RayCaster.RayIntersectsGrid(position, dirAngle, 32, out hitData,
//                    currentMap.GetIsTileOccupiedFunction("walls1"), 16))
//                {
//                    if (hitData.rayLength >= 0)
//                        velocity.Normalize();
//                }
//                
////                velocity = currentMap.Move(velocity, position);
//                position += velocity;
//            }
        }
    }
}