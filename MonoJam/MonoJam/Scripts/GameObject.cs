using Microsoft.Xna.Framework;

namespace MonoJam
{
    public class GameObject
    {
        protected Vector2 Position { get; private set; }

        public void MoveBy(Vector2 pos)
        {
            Position += pos;
        }
    }
}
