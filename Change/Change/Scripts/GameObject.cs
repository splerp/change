using Microsoft.Xna.Framework;

namespace Splerp.Change
{
    public abstract class GameObject
    {
        public Vector2 Position { get; private set; }

        public void MoveBy(Vector2 pos)
        {
            Position += pos;
        }
        public void SetX(float x)
        {
            Position = new Vector2(x, Position.Y);
        }
        public void SetY(float y)
        {
            Position = new Vector2(Position.X, y);
        }
    }
}
