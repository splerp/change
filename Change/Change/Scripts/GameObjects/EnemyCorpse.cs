using Microsoft.Xna.Framework;

namespace Splerp.Change.GameObjects
{
    public sealed class EnemyCorpse : GameObject
    {
        public float gravity = 0.05f;
        public Vector2 speed;

        // Keeps a reference to the enemy it was created for.
        public Enemy EnemyReference;

        // How quickly the death animation should play.
        private float animationSpeed = 6;
        private float animationCount;
        public int animationFrame = 0;

        public bool ReadyToRemove => Position.Y - 5 > ChangeGame.WINDOW_HEIGHT;

        public EnemyCorpse(Enemy enemy)
        {
            EnemyReference = enemy;

            SetX(enemy.CollisionRect.X);
            SetY(enemy.CollisionRect.Y);

            animationCount = animationSpeed;
            
            speed = enemy.Speed + (enemy.Offset - enemy.PreviousOffset);
        }

        public void Update()
        {
            speed += new Vector2(0, gravity);

            MoveBy(speed);

            // Update selected animation frame.
            // TODO: Based on time passed
            animationCount--;
            if (animationCount <= 0)
            {
                animationCount += animationSpeed;
                animationFrame++;
            }
        }
    }
}
