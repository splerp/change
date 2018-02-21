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

        public int selectedOption;

        public GameOverMenuController(GameController gcIn)
        {
            gc = gcIn;
        }

        public void Drop()
        {
            speed = 5;
            currentY = -MonoJam.WINDOW_HEIGHT;

            selectedOption = 0;
        }

        public void Update()
        {
            if (Control.MoveUp.IsJustPressed)
            {
                selectedOption = Math.Max(0, (selectedOption - 1));

                SoundController.Play(Sound.Bip1);
            }
            else if (Control.MoveDown.IsJustPressed)
            {
                selectedOption = Math.Min(TOTAL_OPTIONS - 1, (selectedOption + 1));

                SoundController.Play(Sound.Bip1);
            }

            if(Control.Confirm.IsJustPressed)
            {
                SoundController.Play(Sound.Bip2);

                switch (selectedOption)
                {
                    case 0:
                        gc.SetState(GameState.Playing);
                        break;
                    case 1:
                        gc.SetState(GameState.Title);
                        break;
                }
            }

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
