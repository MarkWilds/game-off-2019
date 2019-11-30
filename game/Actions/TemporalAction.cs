namespace game.Actions
{
    public class TemporalAction : ActionDecorator<float>
    {
        private bool Completed { get; set; }

        private float duration;
        private float timer;

        public TemporalAction(float duration, IAction<float> action = null) : base(action)
        {
            this.duration = duration;
        }

        public override void Restart()
        {
            delegateAction?.Restart();
            
            timer = 0;
            Completed = false;
        }

        protected override bool Update(float dt)
        {
            delegateAction?.Act(Completed ? 1.0f : timer / duration);
            
            if (Completed)
                return true;

            timer += dt;
            Completed = timer > duration;

            return Completed;
        }
    }
}