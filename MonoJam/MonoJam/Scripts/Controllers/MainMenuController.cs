using Microsoft.Xna.Framework.Input;
using System;

namespace MonoJam.Controllers
{
    public class MainMenuController
    {
        public const int TOTAL_OPTIONS = 3;

        private GameController gc;

        public bool previousUp;
        public bool previousDown;
        public bool previousSelect;

        public int selectedOption = 0;

        public MainMenuController(GameController gcIn)
        {
            gc = gcIn;
        }

        public void Update()
        {
            var state = Keyboard.GetState();

            bool upPressed = state.IsKeyDown(Keys.W) || state.IsKeyDown(Keys.Up);
            bool downPressed = state.IsKeyDown(Keys.S) || state.IsKeyDown(Keys.Down);
            bool selectPressed = (state.IsKeyDown(Keys.Space) || state.IsKeyDown(Keys.Enter));

            if (upPressed && !previousUp)
            {
                selectedOption = Math.Max(0, (selectedOption - 1));
            }
            else if (downPressed && !previousDown)
            {
                selectedOption = Math.Min(TOTAL_OPTIONS - 1, (selectedOption + 1));
            }

            if(selectPressed && !previousSelect)
            {
                switch(selectedOption)
                {
                    case 0:
                        gc.StartGame();
                        break;
                    case 1:
                        // Nothing.
                        break;
                    case 2:
                        gc.Exit();
                        break;
                }
            }

            previousUp = upPressed;
            previousDown = downPressed;
            previousSelect = selectPressed;
        }
    }
}
