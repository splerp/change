using Microsoft.Xna.Framework;

namespace MonoJam.GameObjects
{
    public class EnemyCorpse : GameObject
    {
        public float gravity = 0.05f;
        public Vector2 speed;

        public Enemy EnemyReference;

        private float animationSpeed = 6;
        private float animationCount;
        public int animationFrame = 0;

        public bool ReadyToRemove => Position.Y - 5 > MonoJam.WINDOW_HEIGHT;

        public EnemyCorpse(Enemy enemy)
        {
            EnemyReference = enemy;

            SetX(enemy.CollisionRect.X);
            SetY(enemy.CollisionRect.Y);

            animationCount = animationSpeed;
            
            speed = new Vector2(enemy.thrust, enemy.yOffsetDiff);
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
