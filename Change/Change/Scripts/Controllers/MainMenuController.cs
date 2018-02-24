﻿using System;

namespace Splerp.Change.Controllers
{
    public sealed class MainMenuController
    {
        public const int TOTAL_OPTIONS = 3;

        private ChangeGame cg;
        private GameController gc;

        public bool previousUp;
        public bool previousDown;
        public bool previousSelect;

        public int selectedOption = 0;

        public MainMenuController(ChangeGame cgIn, GameController gcIn)
        {
            cg = cgIn;
            gc = gcIn;
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
                        gc.SetState(GameState.MapControls);
                        break;
                    case 2:
                        cg.Exit();
                        break;
                }
            }
        }
    }
}
