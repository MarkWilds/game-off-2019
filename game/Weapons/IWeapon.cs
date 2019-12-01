using DefaultEcs;

namespace game.Weapons
{
    public interface IWeapon
    {
        void Initialize(World world, in Entity weapon);
    }
}