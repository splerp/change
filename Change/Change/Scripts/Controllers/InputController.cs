using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace Splerp.Change.Controllers
{
    public sealed class InputController
    {
        public enum ControlType { Mouse, Keyboard, GamePad }
        public enum MouseButtons { Left, Right }

        // Keep track of input states.
        public static KeyboardState currentKeyboard;
        public static GamePadState currentGamepad;
        public static MouseState currentMouse;

        // Keep track of mouse position.
        public static Point CurrentMousePosition =>
            currentMouse.Position / new Point(ChangeGame.SCALE) - new Point(0, ChangeGame.PLAYABLE_AREA_Y);
        
        #region Remapping variables
        public Control[] MappingOrder = new Control[] {
            Control.MoveUp, Control.MoveDown, Control.MoveLeft, Control.MoveRight
        };

        // Index of the key currently being remapped.
        public int currentRemapIndex;
        public Control CurrentRemappingControl => MappingOrder[currentRemapIndex];

        // Keep track of last frame's key presses (avoids remapping multiple controls)
        public bool pressingKeyboardLastFrame;

        public bool FinishedRemapping => currentRemapIndex >= MappingOrder.Length;
        #endregion

        // Update input states.
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

            // They've just started remapping, so ignore any already-pressed keys.
            pressingKeyboardLastFrame = true;
        }

        // Keep track of current control to override and listen for input.
        public void UpdateMapControls(GameController gameController)
        {
            // Should properly support all supported modes (mouse, gamepad), not just keyboard.
            var pressedKeys = Keyboard.GetState().GetPressedKeys();
            var pressingKeyboard = pressedKeys.Length > 0;

            if (FinishedRemapping)
            {
                gameController.SetState(GameState.Title);
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
}
