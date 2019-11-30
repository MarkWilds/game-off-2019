using System.Collections.Generic;

namespace game.Actions
{
    public class SequenceAction<T> : IAction<T>
    {
        private int currentIndex;
        private readonly List<IAction<T>> actionList;

        public SequenceAction(params IAction<T>[] actions)
        {
            actionList = new List<IAction<T>>();
            
            if(actions.Length >= 0)
                actionList.AddRange(actions);
        }

        public void AddAction(IAction<T> action)
        {
            actionList.Add(action);
        }

        public void Restart()
        {
            actionList.ForEach(a => a.Restart());
            currentIndex = 0;
        }

        public bool Act(T context)
        {
            if (actionList[currentIndex].Act(context))
                currentIndex++;

            return currentIndex >= actionList.Count;
        }
    }
}