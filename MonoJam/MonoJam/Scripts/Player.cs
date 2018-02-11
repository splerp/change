using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace MonoJam
{
    public class Player : GameObject, ICollisionObject
    {
        public Point Size => new Point(8, 8);
        public Rectangle CollisionRect => new Rectangle(new Point(
            (int)Math.Round(Position.X),
            (int)Math.Round(Position.Y)), Size);

        public Vector2 speed;
        public float thrust = 0.1f;
        public float friction = 0.9f;

        public bool FiringLaser { get; set; }
        
        public void Update()
        {
            var kbs = Keyboard.GetState();
            var ms = Mouse.GetState();

            FiringLaser = ms.LeftButton == ButtonState.Pressed;

            var inputVector = new Vector2(
                kbs.IsKeyDown(Keys.A) ? -1 : kbs.IsKeyDown(Keys.D) ? 1 : 0,
                kbs.IsKeyDown(Keys.W) ? -1 : kbs.IsKeyDown(Keys.S) ? 1 : 0) * thrust;

            speed += inputVector;
            speed *= friction;

            MoveBy(speed);

            RestrictToBounds();

            PrintPlayerInfo();
        }

        private void RestrictToBounds()
        {
            if (Position.X < 0)
            {
                SetX(0);
                speed.X *= -1;
            }
            else if (Position.X + Size.X > MonoJam.WINDOW_WIDTH)
            {
                SetX(MonoJam.WINDOW_WIDTH - Size.X);
                speed.X *= -1;
            }
            if (Position.Y < 0)
            {
                SetY(0);
                speed.Y *= -1;
            }
            else if (Position.Y + Size.Y > MonoJam.WINDOW_HEIGHT)
            {
                SetY(MonoJam.WINDOW_HEIGHT - Size.Y);
                speed.Y *= -1;
            }
        }

        private void PrintPlayerInfo()
        {
            Console.WriteLine($"PLAYER. pos: {Position.ToPoint().ToString()}; speed: {speed.ToPoint().ToString()}");
        }
    }
}
