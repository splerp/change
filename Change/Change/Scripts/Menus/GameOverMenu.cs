using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;

namespace Splerp.Change.Menus
{
    public sealed class GameOverMenu : Menu
    {
        private Vector2 speed;
        private Vector2 gravity;

        public override int TotalOptions => 2;

        private GameController gameController;

        public GameOverMenu(GameController gameControllerIn)
        {
            gameController = gameControllerIn;
            Drop();
        }

        // Begin the fall / bounce animation.
        public void Drop()
        {
            // Reset speed on drop.
            speed = new Vector2(0, 300f);
            gravity = new Vector2(0, 700f);

            MenuOffset = new Vector2(0, -ChangeGame.WINDOW_HEIGHT);

            SelectedOption = 0;
        }

        // Drop / bounce the menu.
        public override void OnUpdate(GameTime gameTime)
        {
            speed += gravity * (float)gameTime.ElapsedGameTime.TotalSeconds;
            MenuOffset += speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(MenuOffset.Y > 0)
            {
                MenuOffset = new Vector2(MenuOffset.X, 0);
                speed *= -0.5f;
            }
        }

        public override void OnSelectOption(int selectedOption)
        {
            switch (selectedOption)
            {
                case 0:
                    // Start game.
                    gameController.SetState(GameState.Playing);
                    break;
                case 1:
                    // Back to title.
                    gameController.SetState(GameState.Title);
                    break;
            }
        }
    }
}
