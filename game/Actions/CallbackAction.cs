using System;

namespace game.Actions
{
    public class CallbackAction<T> : IAction<T>
    {
        public event Func<T, bool> OnAct;
        public event Action OnRestart;
        
        public bool Act(T context)
        {
            return OnAct == null || OnAct.Invoke(context);
        }

        public void Restart()
        {
            OnRestart?.Invoke();
        }
    }
}