using Microsoft.Xna.Framework.Input;
using Splerp.Change.Controllers;
using System.Collections.Generic;

namespace Splerp.Change
{
    // Defines a key / button the player can press that is recognisable by the game.
    public sealed class Control
    {
        private bool _prevDownState;

        public static List<Control> PlayerControls = new List<Control>();
        
        // The keys / buttons that can be pressed to activate this control.
        public Keys[] KeyCodes { get; set; } = new Keys[0];
        public Buttons[] ButtonCodes { get; set; } = new Buttons[0];
        public InputController.MouseButtons[] MouseButtonCodes { get; set; } = new InputController.MouseButtons[0];

        // True when the control is currently pressed.
        public bool IsDown { get; private set; }

        // True for the first frame that the control is pressed for.
        public bool IsJustPressed { get; private set; }

        #region Update methods
        public void UpdateControl()
        {
            bool isPressed = false;

            foreach (var k in KeyCodes)
            {
                if (InputController.currentKeyboard.IsKeyDown(k))
                {
                    isPressed = true;
                }
            }
            foreach (var k in ButtonCodes)
            {
                if (InputController.currentGamepad.IsButtonDown(k))
                {
                    isPressed = true;
                }
            }
            foreach (var k in MouseButtonCodes)
            {
                if (k == InputController.MouseButtons.Left && InputController.currentMouse.LeftButton == ButtonState.Pressed)
                {
                    isPressed = true;
                }
                else if (k == InputController.MouseButtons.Right && InputController.currentMouse.RightButton == ButtonState.Pressed)
                {
                    isPressed = true;
                }
            }

            IsDown = isPressed;
            IsJustPressed = isPressed && !_prevDownState;

            _prevDownState = IsDown;
        }
        #endregion

        #region Constructors
        private Control(params Keys[] defaultKeys)
        {
            KeyCodes = defaultKeys;

            PlayerControls.Add(this);
        }

        private Control(params Buttons[] defaultKeys)
        {
            ButtonCodes = defaultKeys;

            PlayerControls.Add(this);
        }

        private Control(params InputController.MouseButtons[] defaultKeys)
        {
            MouseButtonCodes = defaultKeys;

            PlayerControls.Add(this);
        }
        #endregion

        #region Control definitions
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
        #endregion
    }
}
