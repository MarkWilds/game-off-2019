using DefaultEcs;

namespace game.Extensions
{
    public static class EntityExtensions
    {
        public static Entity GetFirst(this EntitySet lhs)
        {
            return lhs.GetEntities()[0];
        }
    }
}