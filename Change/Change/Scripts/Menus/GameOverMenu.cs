using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;

namespace Splerp.Change.Menus
{
    public sealed class GameOverMenu : Menu
    {
        private Vector2 speed;
        private Vector2 gravity;

        public override int TotalOptions => 2;

        private GameController gc;

        public GameOverMenu(GameController gcIn)
        {
            gc = gcIn;
            Drop();
        }

        // Begin the fall / bounce animation.
        public void Drop()
        {
            // Reset speed on drop.
            speed = new Vector2(0, 5);
            gravity = new Vector2(0, 0.2f);

            MenuOffset = new Vector2(0, -ChangeGame.WINDOW_HEIGHT);

            SelectedOption = 0;
        }

        // Drop / bounce the menu.
        public override void OnUpdate()
        {
            speed += gravity;
            MenuOffset += speed;

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
                    gc.SetState(GameState.Playing);
                    break;
                case 1:
                    // Back to title.
                    gc.SetState(GameState.Title);
                    break;
            }
        }
    }
}
