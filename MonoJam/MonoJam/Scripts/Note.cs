using Microsoft.Xna.Framework;

namespace MonoJam
{
    public class Note : GameObject, ICollisionObject, IHurtable
    {
        public const int WIDTH = 8;
        public const int HEIGHT = 4;

        public Point Size => new Point(WIDTH, HEIGHT);
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        public bool ReadyToRemove => Position.Y > MonoJam.PLAYABLE_AREA_HEIGHT;

        public float downSpeed = 0.1f;

        public int MaxHealth => 1;
        public int CurrentHealth { get; set; }
        public bool IsDead => CurrentHealth <= 0;

        public Note()
        {
            SetX(GameController.random.Next(0, MonoJam.WINDOW_WIDTH - WIDTH));
            SetY(-HEIGHT);

            CurrentHealth = MaxHealth;
        }

        public void Update()
        {
            MoveBy(new Vector2(0, downSpeed));
        }

        public void Damage(int amount)
        {
            CurrentHealth -= amount;
        }
    }
}
