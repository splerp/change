using Microsoft.Xna.Framework;

namespace MonoJam
{
    class Enemy : GameObject, ICollisionObject
    {
        public Point Size => new Point(15, 15);
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);
    }
}
