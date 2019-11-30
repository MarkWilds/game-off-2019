using System;

namespace game.StateMachine
{
    /// <summary>
    /// Non-generic state interface.
    /// </summary>
    public interface IState
    {
        string Identifier { get; }
        
        IState ActiveState { get; }
        
        /// <summary>
        /// Parent state, or null if this is the root level state.
        /// </summary>
        IState Parent { get; set; }

        /// <summary>
        /// Change to the state with the specified name.
        /// </summary>
        void ChangeState(string stateName);

        /// <summary>
        /// Push another state above the current one, so that popping it will return to the
        /// current state.
        /// </summary>
        void PushState(string stateName);

        /// <summary>
        /// Exit out of the current state and enter whatever state is below it in the stack.
        /// </summary>
        void PopState();

        /// <summary>
        /// Update this state and its children with a specified delta time.
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// Triggered when we enter the state.
        /// </summary>
        void Enter();

        /// <summary>
        /// Triggered when we exit the state.
        /// </summary>
        void Exit();

        /// <summary>
        /// Trigger an event on this state or one of its children.
        /// </summary>
        /// <param name="name">Name of the event to trigger</param>
        void TriggerEvent(string name);

        /// <summary>
        /// Triggered when and event occurs. Executes the event's action if the 
        /// current state is at the top of the stack, otherwise triggers it on 
        /// the next state down.
        /// </summary>
        /// <param name="name">Name of the event to trigger</param>
        /// <param name="eventArgs">Arguments to send to the event</param>
        void TriggerEvent(string name, EventArgs eventArgs);
    }
}