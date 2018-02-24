using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;
using System;

namespace Splerp.Change.GameObjects
{
    public sealed class PaddlePlayer : GameObject, ICollisionObject
    {
        private GameController gc;

        public const int WIDTH = 14;
        public const int HEIGHT = 5;
        public const int GRAPHIC_EDGE_WIDTH = 1;

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
            SetX((ChangeGame.PLAYABLE_AREA_WIDTH - Size.X) / 2);
            SetY(ChangeGame.PLAYABLE_AREA_HEIGHT + ChangeGame.PADDLE_AREA_HEIGHT - HEIGHT);
            speed = Vector2.Zero;
        }

        public void Update()
        {
            int input = Control.MoveLeft.IsDown ? -1 : Control.MoveRight.IsDown ? 1 : 0;
            Vector2 inputVector = new Vector2(input, 0) * thrust;

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
            else if (Position.X + Size.X > ChangeGame.PLAYABLE_AREA_WIDTH)
            {
                SetX(ChangeGame.PLAYABLE_AREA_WIDTH - Size.X);
                speed.X *= -1;
            }
        }

        private void PrintPlayerInfo()
        {
            //Console.WriteLine($"PLAYER PADDLE. pos: {Position.ToPoint().ToString()}; speed: {speed.ToPoint().ToString()}");
        }
    }
}
