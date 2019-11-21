using System;
using Microsoft.Xna.Framework;

namespace game.Screens
{
    public enum ScreenState
    {
        TransitionOn,
        Active,
        TransitionOff,
        Hidden,
    }

    public abstract class GameScreen
    {
        private ScreenManager screenManager;
        private TimeSpan transitionOnTime = TimeSpan.Zero;
        private TimeSpan transitionOffTime = TimeSpan.Zero;
        private ScreenState screenState = ScreenState.TransitionOn;
        
        private bool isPopup;
        private bool isExiting;
        private bool hasToBeRemoved;
        private bool otherScreenHasFocus;
        private float transitionPosition = 1;

        public TimeSpan TransitionOnTime
        {
            get => transitionOnTime;
            protected set => transitionOnTime = value;
        }

        public TimeSpan TransitionOffTime
        {
            get => transitionOffTime;
            protected set => transitionOffTime = value;
        }

        public float TransitionPosition
        {
            get => transitionPosition;
            protected set => transitionPosition = value;
        }

        public bool IsPopup
        {
            get => isPopup;
            protected set => isPopup = value;
        }

        public byte TransitionAlpha => (byte) (255 - TransitionPosition * 255);

        public bool IsActive => !otherScreenHasFocus && (screenState == ScreenState.TransitionOn ||
                                                         screenState == ScreenState.Active);

        public bool IsTransitioning => screenState == ScreenState.TransitionOn || screenState == ScreenState.TransitionOff;

        public bool IsExiting
        {
            get => isExiting;
            protected internal set => isExiting = value;
        }

        public bool HasToBeRemoved => hasToBeRemoved;

        public ScreenState ScreenState
        {
            get => screenState;
            protected set => screenState = value;
        }

        public ScreenManager ScreenManager
        {
            get => screenManager;
            protected internal set => screenManager = value;
        }
        
        public virtual void LoadContent() { }
        public virtual void UnloadContent() { }
        public virtual bool HandleInput()
        {
            return false;
        }

        public abstract void Draw(GameTime gameTime);

        public virtual void Update(GameTime gameTime, bool otherScreenHasFocus,
            bool coveredByOtherScreen)
        {
            this.otherScreenHasFocus = otherScreenHasFocus;

            if (isExiting)
            {
                // If the screen is going away to die, it should transition off.
                screenState = ScreenState.TransitionOff;

                if (!UpdateTransition(gameTime, transitionOffTime, 1))
                {
                    // When the transition finishes, remove the screen.
                    ScreenManager.RemoveScreen(this);
                }
            }
            else if (coveredByOtherScreen)
            {
                // If the screen is covered by another, it should transition off.
                screenState = UpdateTransition(gameTime, transitionOffTime, 1) ? ScreenState.TransitionOff 
                    : ScreenState.Hidden;
            }
            else
            {
                // Otherwise the screen should transition on and become active.
                screenState = UpdateTransition(gameTime, transitionOnTime, -1) ? ScreenState.TransitionOn 
                    : ScreenState.Active;
            }
        }
        
        private bool UpdateTransition(GameTime gameTime, TimeSpan time, int direction)
        {
            // How much should we move by?
            float transitionDelta;

            if (time == TimeSpan.Zero)
                transitionDelta = 1;
            else
                transitionDelta = (float) (gameTime.ElapsedGameTime.TotalMilliseconds /
                                           time.TotalMilliseconds);

            // Update the transition position.
            transitionPosition += transitionDelta * direction;

            // Did we reach the end of the transition?
            if (((direction < 0) && (transitionPosition <= 0)) ||
                ((direction > 0) && (transitionPosition >= 1)))
            {
                transitionPosition = MathHelper.Clamp(transitionPosition, 0, 1);
                return false;
            }

            // Otherwise we are still busy transitioning.
            return true;
        }

        public void ExitScreen()
        {
            if (TransitionOffTime == TimeSpan.Zero)
                hasToBeRemoved = true;
            else
                isExiting = true;
        }
    }
}