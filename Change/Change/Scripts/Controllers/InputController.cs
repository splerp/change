using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace MonoJam.Controllers
{
    public sealed class InputController
    {
        public enum ControlType { Mouse, Keyboard, GamePad }
        public enum MouseButtons { Left, Right }

        public static KeyboardState currentKeyboard;
        public static GamePadState currentGamepad;
        public static MouseState currentMouse;
        public static Point CurrentMousePosition =>
            currentMouse.Position / new Point(MonoJam.SCALE) - new Point(0, MonoJam.PLAYABLE_AREA_Y);
        
        #region Remapping variables
        public Control[] MappingOrder = new Control[] {
            Control.MoveUp, Control.MoveDown, Control.MoveLeft, Control.MoveRight
        };

        // Index of the key currently being remapped.
        public int currentRemapIndex;
        public Control CurrentRemappingControl => MappingOrder[currentRemapIndex];

        // Keep track of last frame's key presses (avoids remapping multiple controls 
        public bool pressingKeyboardLastFrame;

        public bool FinishedRemapping => currentRemapIndex >= MappingOrder.Length;
        #endregion

        public void UpdateControlStates()
        {
            currentKeyboard = Keyboard.GetState();
            currentGamepad = GamePad.GetState(0);
            currentMouse = Mouse.GetState();

            foreach (var control in Control.PlayerControls)
            {
                control.UpdateControl();
            }
        }

        #region Remapping methods
        public void StartRemapping()
        {
            currentRemapIndex = 0;
        }

        // Keep track of current control to override and listen for input.
        public void UpdateMapControls(GameController gc)
        {
            // Should properly support all supported modes (mouse, gamepad), not just keyboard.
            var pressedKeys = Keyboard.GetState().GetPressedKeys();
            var pressingKeyboard = pressedKeys.Length > 0;

            if (FinishedRemapping)
            {
                gc.SetState(GameState.Title);
            }
            else
            {
                if (pressingKeyboard && !pressingKeyboardLastFrame)
                {
                    RemapControl(pressedKeys);
                }
            }

            pressingKeyboardLastFrame = pressingKeyboard;
        }
        
        public void RemapControl(Keys[] newKeys)
        {
            // If escape was pressed, do not map this key.
            if(!newKeys.Any(k => k == Keys.Escape))
            {
                CurrentRemappingControl.KeyCodes = newKeys;
                SoundController.Play(Sound.Bip2);
            }
            else
            {
                SoundController.Play(Sound.Bip1);
            }
            
            currentRemapIndex++;
        }
        #endregion
    }

    public class Control
    {
        private bool _prevDownState;

        public Keys[] KeyCodes { get; set; } = new Keys[0];
        public Buttons[] ButtonCodes { get; set; } = new Buttons[0];
        public InputController.MouseButtons[] MouseButtonCodes { get; set; } = new InputController.MouseButtons[0];
        public bool IsDown { get; private set; }
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
