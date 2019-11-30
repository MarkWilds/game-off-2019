namespace game.Actions
{
    public interface IAction<in T>
    {
        bool Act(T context);

        void Restart();
    }
}