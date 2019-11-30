namespace game.Actions
{
    public abstract class ActionDecorator<T> : IAction<T>
    {
        protected readonly IAction<T> delegateAction;

        protected ActionDecorator(IAction<T> action)
        {
            delegateAction = action;
        }
        
        public bool Act(T context)
        {
            return Update(context);
        }

        protected abstract bool Update(T context);
        
        public virtual void Restart()
        {
            delegateAction.Restart();
        }
    }
}