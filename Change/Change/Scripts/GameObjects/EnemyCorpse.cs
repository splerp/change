using Microsoft.Xna.Framework;

namespace Splerp.Change.GameObjects
{
    public sealed class EnemyCorpse : GameObject
    {
        public float gravity = 100f;
        public Vector2 speed;

        // Keeps a reference to the enemy it was created for.
        public Enemy EnemyReference;

        // How quickly the death animation should play.
        private float animationSpeed = 0.1f;
        private float animationCount;
        public int animationFrame = 0;

        public bool ReadyToRemove => Position.Y - 5 > ChangeGame.WINDOW_HEIGHT;

        public EnemyCorpse(Enemy enemy)
        {
            EnemyReference = enemy;

            SetX(enemy.CollisionRect.X);
            SetY(enemy.CollisionRect.Y);

            animationCount = animationSpeed;
            
            speed = enemy.Speed + ((enemy.Offset - enemy.PreviousOffset) * 100);
        }

        public void Update(GameTime gameTime)
        {
            speed += new Vector2(0, gravity * (float)gameTime.ElapsedGameTime.TotalSeconds);

            MoveBy(speed * (float)gameTime.ElapsedGameTime.TotalSeconds);

            // Update selected animation frame.
            animationCount -= (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (animationCount <= 0)
            {
                animationCount += animationSpeed;
                animationFrame++;
            }
        }
    }
}
