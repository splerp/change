﻿using Microsoft.Xna.Framework;

namespace MonoJam
{
    public class GameObject
    {
        protected Vector2 Position { get; private set; }

        public void MoveBy(Vector2 pos)
        {
            Position += pos;
        }
        public void SetX(int x)
        {
            Position = new Vector2(x, Position.Y);
        }
        public void SetY(int y)
        {
            Position = new Vector2(Position.X, y);
        }
    }
}
