using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;

namespace Splerp.Change.Menus
{
    public sealed class MainMenu : Menu
    {
        private ChangeGame cg;
        private GameController gc;

        public override int TotalOptions => 3;

        public MainMenu(ChangeGame cgIn, GameController gcIn)
        {
            cg = cgIn;
            gc = gcIn;
        }

        public override void OnUpdate(GameTime gameTime) { }

        public override void OnSelectOption(int selectedOption)
        {
            switch (selectedOption)
            {
                case 0:
                    // Start playing.
                    gc.SetState(GameState.Playing);
                    break;
                case 1:
                    // Remap the controls.
                    gc.SetState(GameState.MapControls);
                    break;
                case 2:
                    // Exit the game.
                    cg.Exit();
                    break;
            }
        }
    }
}
