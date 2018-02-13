using Microsoft.Xna.Framework;

namespace MonoJam
{
    public class Note : GameObject, ICollisionObject
    {
        public const int WIDTH = 8;
        public const int HEIGHT = 4;

        public Point Size => new Point(WIDTH, HEIGHT);
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        public bool ReadyToRemove => Position.Y > MonoJam.PLAYABLE_AREA_HEIGHT;
        public bool Destroyed { get; set; }

        public float downSpeed = 0.1f;

        public Note()
        {
            SetX(GameController.random.Next(0, MonoJam.WINDOW_WIDTH - WIDTH));
            SetY(-HEIGHT);
        }

        public void Update()
        {
            MoveBy(new Vector2(0, downSpeed));
        }
    }
}
