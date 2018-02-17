using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using MonoJam.Controllers;
using System;

namespace MonoJam.GameObjects
{
    public class PaddlePlayer : GameObject, ICollisionObject
    {
        private GameController gc;

        public const int WIDTH = 14;
        public const int HEIGHT = 5;

        public Point Size => new Point(WIDTH, HEIGHT);
        public Rectangle CollisionRect => new Rectangle(new Point(
            (int)Math.Round(Position.X),
            (int)Math.Round(Position.Y)), Size);

        public Vector2 speed;
        public float thrust = 0.1f;
        public float friction = 0.9f;
        
        public PaddlePlayer(GameController gcIn)
        {
            gc = gcIn;

            Reset();
        }

        public void Reset()
        {
            SetX(0);
            SetY(MonoJam.PLAYABLE_AREA_HEIGHT);
            speed = Vector2.Zero;
        }

        public void Update()
        {
            var kbs = Keyboard.GetState();

            Vector2 inputVector = new Vector2(kbs.IsKeyDown(Keys.A) ? -1 : kbs.IsKeyDown(Keys.D) ? 1 : 0, 0) * thrust;

            speed += inputVector;
            speed *= friction;

            MoveBy(speed);

            RestrictToBounds();
            
            PrintPlayerInfo();
        }

        private void RestrictToBounds()
        {
            // Only need to check X.
            if (Position.X < 0)
            {
                SetX(0);
                speed.X *= -1;
            }
            else if (Position.X + Size.X > MonoJam.PLAYABLE_AREA_WIDTH)
            {
                SetX(MonoJam.PLAYABLE_AREA_WIDTH - Size.X);
                speed.X *= -1;
            }
        }

        private void PrintPlayerInfo()
        {
            Console.WriteLine($"PLAYER PADDLE. pos: {Position.ToPoint().ToString()}; speed: {speed.ToPoint().ToString()}");
        }
    }
}
