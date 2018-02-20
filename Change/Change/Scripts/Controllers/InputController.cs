using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

namespace MonoJam.Controllers
{
    public class InputController
    {
        public enum ControlType { Mouse, Keyboard, GamePad }
        public enum MouseButtons { Left, Right }

        private static KeyboardState currentKeyboard;
        private static GamePadState currentGamepad;
        private static MouseState currentMouse;

        public static Point CurrentMousePosition => currentMouse.Position / new Point(MonoJam.SCALE) - new Point(0, MonoJam.PLAYABLE_AREA_Y);

        public void UpdateControlStates()
        {
            currentKeyboard = Keyboard.GetState();
            currentGamepad = GamePad.GetState(0);
            currentMouse = Mouse.GetState();

            foreach (var control in Control.PlayerControls)
            {
                switch (control.ControlType)
                {
                    case ControlType.Keyboard: control.UpdateControl(currentKeyboard); break;
                    case ControlType.GamePad: control.UpdateControl(currentGamepad); break;
                    case ControlType.Mouse: control.UpdateControl(currentMouse); break;
                }
            }
        }
    }

    public class Control
    {
        private bool _prevDownState;

        public InputController.ControlType ControlType { get; private set; }

        public Keys[] KeyCodes { get; private set; }
        public Buttons[] ButtonCodes { get; private set; }
        public InputController.MouseButtons[] MouseButtonCodes { get; private set; }
        public bool IsDown { get; private set; }
        public bool IsJustPressed { get; private set; }

        #region Update methods
        public void UpdateControl(KeyboardState state)
        {
            bool isPressed = false;

            foreach (var k in KeyCodes)
            {
                if (state.IsKeyDown(k))
                {
                    isPressed = true;
                }
            }

            IsDown = isPressed;
            IsJustPressed = isPressed && !_prevDownState;

            _prevDownState = IsDown;
        }

        public void UpdateControl(GamePadState state)
        {
            bool isPressed = false;

            foreach (var k in ButtonCodes)
            {
                if (state.IsButtonDown(k))
                {
                    isPressed = true;
                }
            }

            IsDown = isPressed;
            if (!_prevDownState)
            {
                IsJustPressed = isPressed;
            }

            _prevDownState = IsDown;
        }

        public void UpdateControl(MouseState state)
        {
            bool isPressed = false;

            foreach (var k in MouseButtonCodes)
            {
                if (k == InputController.MouseButtons.Left && state.LeftButton == ButtonState.Pressed)
                {
                    isPressed = true;
                }
                else if (k == InputController.MouseButtons.Right && state.RightButton == ButtonState.Pressed)
                {
                    isPressed = true;
                }
            }

            IsDown = isPressed;
            if (!_prevDownState)
            {
                IsJustPressed = isPressed;
            }

            _prevDownState = IsDown;
        }
        #endregion

        #region Constructors
        private Control(params Keys[] defaultKeys)
        {
            KeyCodes = defaultKeys;
            ControlType = InputController.ControlType.Keyboard;

            PlayerControls.Add(this);
        }

        private Control(params Buttons[] defaultKeys)
        {
            ButtonCodes = defaultKeys;
            ControlType = InputController.ControlType.GamePad;

            PlayerControls.Add(this);
        }

        private Control(params InputController.MouseButtons[] defaultKeys)
        {
            MouseButtonCodes = defaultKeys;
            ControlType = InputController.ControlType.Mouse;

            PlayerControls.Add(this);
        }
        #endregion

        public static List<Control> PlayerControls = new List<Control>();

        public static Control MoveUp = new Control(Keys.W, Keys.Up);
        public static Control MoveDown = new Control(Keys.S, Keys.Down);
        public static Control MoveLeft = new Control(Keys.A, Keys.Left);
        public static Control MoveRight = new Control(Keys.D, Keys.Right);
        public static Control Attack = new Control(InputController.MouseButtons.Left);
        public static Control Confirm = new Control(Keys.Space, Keys.Enter);
        public static Control Return = new Control(Keys.Escape);
        public static Control MuteMusic = new Control(Keys.M);
        public static Control MuteSound = new Control(Keys.N);
        public static Control SkipTutorial = new Control(Keys.T);
    }
}
