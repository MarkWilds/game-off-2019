using System;

namespace game.Actions
{
    public class MapAction<TF, T> : IAction<T>
    {
        private readonly IAction<TF> delegateAction;
        private readonly Func<T, TF> mapFunction;

        public MapAction(IAction<TF> action, Func<T, TF> func)
        {
            delegateAction = action;
            mapFunction = func;
        }
        
        public bool Act(T context)
        {
            if (mapFunction == null)
                return true;
            
            var value = mapFunction.Invoke(context);
            return delegateAction.Act(value);
        }

        public void Restart()
        {
            delegateAction.Restart();
        }
    }
}