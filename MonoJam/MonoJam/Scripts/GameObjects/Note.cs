using Microsoft.Xna.Framework;
using MonoJam.Controllers;

namespace MonoJam.GameObjects
{
    public class Note : GameObject, ICollisionObject, IHurtable
    {
        public const int WIDTH = 8;
        public const int HEIGHT = 4;

        private GameController gc;

        public Point Size => new Point(WIDTH, HEIGHT);
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        public bool InRangeForCatching => Position.Y < MonoJam.PLAYABLE_AREA_HEIGHT;
        public bool ReadyToRemove => Position.Y > MonoJam.WINDOW_HEIGHT;
        public bool Caught;

        public float downSpeed = 0.1f;

        public int MaxHealth => 1;
        public int CurrentHealth { get; set; }
        public bool IsDead => CurrentHealth <= 0;

        public Note(GameController gcIn)
        {
            gc = gcIn;

            SetX(GameController.random.Next(0, MonoJam.WINDOW_WIDTH - WIDTH));
            SetY(-HEIGHT);

            CurrentHealth = MaxHealth;
        }

        public void Update()
        {
            MoveBy(new Vector2(0, downSpeed));

            if(Caught)
            {
                if (Position.X < gc.paddlePlayer.CollisionRect.X + PaddlePlayer.GRAPHIC_EDGE_WIDTH)
                {
                    SetX(gc.paddlePlayer.CollisionRect.X + PaddlePlayer.GRAPHIC_EDGE_WIDTH);
                }
                else if (Position.X + WIDTH > gc.paddlePlayer.CollisionRect.Right - PaddlePlayer.GRAPHIC_EDGE_WIDTH)
                {
                    SetX(gc.paddlePlayer.CollisionRect.Right - PaddlePlayer.GRAPHIC_EDGE_WIDTH - WIDTH);
                }
            }
        }

        public void Damage(int amount)
        {
            CurrentHealth -= amount;
        }
    }
}
