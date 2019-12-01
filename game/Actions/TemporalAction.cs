namespace game.Actions
{
    public class TemporalAction : ActionDecorator<float>
    {
        private bool Completed { get; set; }

        private float duration;
        private float timer;

        public bool Reverse { get; set; }

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
            if (Completed)
                return true;
            
            var value = Completed ? 1.0f : timer / duration;
            delegateAction?.Act(Reverse ? 1 - value : value);

            timer += dt;
            Completed = timer > duration;

            return Completed;
        }
    }
}