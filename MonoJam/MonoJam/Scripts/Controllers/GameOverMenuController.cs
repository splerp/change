using Microsoft.Xna.Framework.Input;
using System;

namespace MonoJam.Controllers
{
    public class GameOverMenuController
    {
        public const int TOTAL_OPTIONS = 2;

        public float currentY;
        private float speed;
        private float gravity = 0.2f;

        private GameController gc;

        public bool previousUp;
        public bool previousDown;
        public bool previousSelect;

        public int selectedOption = 0;

        public GameOverMenuController(GameController gcIn)
        {
            gc = gcIn;
        }

        public void Drop()
        {
            speed = 5;
            currentY = -MonoJam.WINDOW_HEIGHT;
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
                selectedOption = Math.Min(TOTAL_OPTIONS - 1, (selectedOption + 1));
            }

            if(selectPressed)
            {
                switch(selectedOption)
                {
                    case 0:
                        gc.RestartGame();
                        break;
                    case 1:
                        gc.ToMainMenu();
                        break;
                }
            }

            previousUp = state.IsKeyDown(Keys.W);
            previousDown = state.IsKeyDown(Keys.S);
            previousSelect = state.IsKeyDown(Keys.Space) || state.IsKeyDown(Keys.Enter);

            speed += gravity;
            currentY += speed;

            if(currentY > 0)
            {
                currentY = 0;
                speed *= -0.5f;
            }
        }
    }
}
