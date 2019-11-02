using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace game
{
    public static class InputManager
    {
        private static KeyboardState currentKeyboardState;
        private static KeyboardState previousKeyboardState;

        private static MouseState currentMouseState;
        private static MouseState previousMouseState;

        /// <summary>
        /// The amount the mouse moved in the X axis
        /// </summary>
        public static float MouseAxisX => currentMouseState.X - previousMouseState.X;

        /// <summary>
        /// The amount the mouse moved in the Y axis
        /// </summary>
        public static float MouseAxisY => currentMouseState.Y - previousMouseState.Y;

        /// <summary>
        /// The world position of the mouse
        /// </summary>
        public static Vector2 MousePosition => currentMouseState.Position.ToVector2();
        
        public static bool ScrollWheelUp => currentMouseState.ScrollWheelValue > previousMouseState.ScrollWheelValue;
        public static bool ScrollWheelDown => currentMouseState.ScrollWheelValue < previousMouseState.ScrollWheelValue;

        /// <summary>
        /// Checks if a key is being pressed down this frame
        /// </summary>
        public static bool IsKeyDown(Keys key)
        {
            return currentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Check if a key has been pressed this frame and was released last frame
        /// </summary>
        public static bool IsKeyPressed(Keys key)
        {
            return previousKeyboardState.IsKeyUp(key) && currentKeyboardState.IsKeyDown(key);
        }

        /// <summary>
        /// Check if a mousebutton is being held down this frame
        /// </summary>
        public static bool IsMouseButtonPressed(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return currentMouseState.LeftButton == ButtonState.Pressed;
                case MouseButton.Right:
                    return currentMouseState.RightButton == ButtonState.Pressed;
                default:
                    throw new Exception("That is not a valid mouse button");
            }
        }

        /// <summary>
        /// Check if a mousebutton has been clicked this frame
        /// </summary>
        /// <param name="button"></param>
        /// <returns></returns>
        public static bool MouseButtonClicked(MouseButton button)
        {
            switch (button)
            {
                case MouseButton.Left:
                    return currentMouseState.LeftButton == ButtonState.Pressed && previousMouseState.LeftButton == ButtonState.Released;
                case MouseButton.Right:
                    return currentMouseState.RightButton == ButtonState.Pressed && previousMouseState.RightButton == ButtonState.Released;
                default:
                    throw new Exception("That is not a valid mouse button");
            }
        }

        /// <summary>
        /// Return the horizontal axis input
        /// </summary>
        public static float HorizontalAxis => IsKeyDown(Keys.D) ? 1 : IsKeyDown(Keys.A) ? -1 : 0;

        /// <summary>
        /// Return the vertical axis input
        /// </summary>
        public static float VerticalAxis => IsKeyDown(Keys.W) ? 1 : IsKeyDown(Keys.S) ? -1 : 0;

        public static void Update()
        {            
            previousKeyboardState = currentKeyboardState;
            currentKeyboardState = Keyboard.GetState();

            previousMouseState = currentMouseState;
            currentMouseState = Mouse.GetState();
        }
    }
}
