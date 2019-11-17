using DefaultEcs;

namespace game.ECS.Events
{
    public struct MapLoadedEvent
    {
        public Entity entity;
        public string startingSpawn;
    }
}