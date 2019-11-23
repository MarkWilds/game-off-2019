using System.Collections.Concurrent;
using DefaultEcs;
using DefaultEcs.System;
using game.Data;
using game.ECS.Events;
using Humper;
using Microsoft.Xna.Framework;
using World = DefaultEcs.World;

namespace game.ECS.Systems
{
    public class TriggerResolveSystem : ISystem<GameTime>
    {
        private readonly World ecsContext;

        public bool IsEnabled { get; set; }

        private readonly ConcurrentQueue<ICollision> collisions;

        public TriggerResolveSystem(World world)
        {
            ecsContext = world;
            ecsContext.Subscribe(this);
            collisions = new ConcurrentQueue<ICollision>();
        }

        [Subscribe]
        private void OnCollisionResolveEvent(in ICollision @event)
        {
            collisions.Enqueue(@event);
        }

        public void Update(GameTime state)
        {
            foreach (var collision in collisions)
            {
                if (collision.Box.HasTag(CollisionTag.Player) &&
                    collision.Other.Data is TriggerInfo trigger)
                {
                    switch (trigger.type)
                    {
                        case TriggerType.ChangeMap:
                        {
                            var mapInfo = new MapInfo()
                                {mapName = trigger.data.map, spawnName = trigger.data.spawn};
                            ecsContext.Publish(new MapLoadEvent() {mapInfo = mapInfo});
                            break;
                        }
                    }
                }
            }

            collisions.Clear();
        }

        public void Dispose()
        {
        }
    }
}