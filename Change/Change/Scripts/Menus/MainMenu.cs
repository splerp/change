using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;

namespace Splerp.Change.Menus
{
    public sealed class MainMenu : Menu
    {
        private ChangeGame game;
        private GameController gameController;

        public override int TotalOptions => 3;

        public MainMenu(ChangeGame gameIn, GameController gameControllerIn)
        {
            game = gameIn;
            gameController = gameControllerIn;
        }

        public override void OnUpdate(GameTime gameTime) { }

        public override void OnSelectOption(int selectedOption)
        {
            switch (selectedOption)
            {
                case 0:
                    // Start playing.
                    gameController.SetState(GameState.Playing);
                    break;
                case 1:
                    // Remap the controls.
                    gameController.SetState(GameState.MapControls);
                    break;
                case 2:
                    // Exit the game.
                    game.Exit();
                    break;
            }
        }
    }
}
