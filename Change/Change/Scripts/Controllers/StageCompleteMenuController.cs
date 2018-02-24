namespace Splerp.Change.Controllers
{
    public sealed class StageCompleteMenuController
    {
        public float currentY;
        private float speed;
        private float gravity;

        private float fallFor;

        private GameController gc;

        public StageCompleteMenuController(GameController gcIn)
        {
            gc = gcIn;
            Drop();
        }

        public void Drop()
        {
            speed = 5;
            currentY = -ChangeGame.WINDOW_HEIGHT;

            gravity = 0.2f;
            fallFor = 140;
        }

        public bool AnimationComplete => fallFor <= 0 && currentY < -ChangeGame.WINDOW_HEIGHT;

        public void Update()
        {
            speed += gravity;
            currentY += speed;

            if(currentY > 0)
            {
                currentY = 0;
                speed *= -0.5f;
            }

            fallFor--;
            if(fallFor <= 0)
            {
                gravity = -0.2f;
            }
        }
    }
}
