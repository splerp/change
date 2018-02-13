using Microsoft.Xna.Framework.Input;
using System;

namespace MonoJam.Controllers
{
    public class MainMenuController
    {
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

            bool upPressed = state.IsKeyDown(Keys.W) && !previousUp;
            bool downPressed = state.IsKeyDown(Keys.S) && !previousDown;
            bool selectPressed = (state.IsKeyDown(Keys.Space) || state.IsKeyDown(Keys.Enter)) && !previousSelect;

            if (upPressed)
            {
                selectedOption = Math.Max(0, (selectedOption - 1));
            }
            else if (downPressed)
            {
                selectedOption = Math.Min(2, (selectedOption + 1));
            }

            if(selectPressed)
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

            previousUp = state.IsKeyDown(Keys.W);
            previousDown = state.IsKeyDown(Keys.S);
            previousSelect = state.IsKeyDown(Keys.Space) || state.IsKeyDown(Keys.Enter);
        }
    }
}
