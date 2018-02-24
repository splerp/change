using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;
using System;

namespace Splerp.Change.Menus
{
    // Base menu class that handles move up / move down / select input.
    public abstract class Menu
    {
        public Vector2 MenuOffset { get; set; }
        public int SelectedOption { get; set; }
        public abstract int TotalOptions { get; }

        public abstract void OnUpdate();
        public abstract void OnSelectOption(int selectedOption);

        public void Update()
        {
            // Avoid playing sounds on input, if menu has no options.
            if(TotalOptions > 0)
            {
                if (Control.MoveUp.IsJustPressed)
                {
                    SelectedOption = Math.Max(0, (SelectedOption - 1));

                    SoundController.Play(Sound.Bip1);
                }
                else if (Control.MoveDown.IsJustPressed)
                {
                    SelectedOption = Math.Min(TotalOptions - 1, (SelectedOption + 1));

                    SoundController.Play(Sound.Bip1);
                }

                if (Control.Confirm.IsJustPressed)
                {
                    SoundController.Play(Sound.Bip2);
                    OnSelectOption(SelectedOption);
                }
            }
            
            OnUpdate();
        }
    }
}
