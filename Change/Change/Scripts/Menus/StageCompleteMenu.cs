using Microsoft.Xna.Framework;

namespace Splerp.Change.Menus
{
    public sealed class StageCompleteMenu : Menu
    {
        private Vector2 speed;
        private Vector2 gravity;

        // How long the menu should be falling before gravity is reversed.
        private float fallFor;

        // No options for this menu.
        public override int TotalOptions => 0;

        public bool AnimationComplete => fallFor <= 0 && MenuOffset.Y < -ChangeGame.WINDOW_HEIGHT;

        public StageCompleteMenu()
        {
            Drop();
        }

        // Begin the fall / bounce animation.
        public void Drop()
        {
            speed = new Vector2(0, 300f);
            gravity = new Vector2(0, 700f);

            MenuOffset = new Vector2(0, -ChangeGame.WINDOW_HEIGHT);

            fallFor = 140;
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

            // If enough time has passed, move menu back up.
            fallFor--;
            if(fallFor <= 0)
            {
                gravity = new Vector2(0, -0.2f);
            }
        }

        public override void OnSelectOption(int selectedOption) { }
    }
}
