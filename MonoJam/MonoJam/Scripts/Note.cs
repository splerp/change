using Microsoft.Xna.Framework;

namespace MonoJam
{
    public class Note : GameObject, ICollisionObject
    {
        public const int WIDTH = 16;
        public const int HEIGHT = 7;

        public Point Size => new Point(WIDTH, HEIGHT);
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        public float downSpeed = 0.3f;

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
