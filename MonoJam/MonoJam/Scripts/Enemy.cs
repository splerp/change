using Microsoft.Xna.Framework;
using System;

namespace MonoJam
{
    public class Enemy : GameObject, ICollisionObject
    {
        public const int WIDTH = 10;
        public const int HEIGHT = 10;

        public Point Size => new Point(WIDTH, HEIGHT);
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        public float thrust = 0.5f;
        public int direction = 1;

        public float yOffsetCount;
        public float yOffset;
        public float yOffsetDiff;
        public float yPos;

        public float sinAmp = 8f;
        public float sinPer = 0.08f;
        
        public bool ReadyToRemove => totalHealth <= 0 || Position.X + Size.X < 0 || Position.X > MonoJam.WINDOW_WIDTH;

        public int totalHealth = 1000;

        public Enemy()
        {
            yOffsetCount = GameController.random.Next(1, 1000);

            direction = GameController.random.Next(1, 3) == 1 ? 1 : -1;
            thrust *= direction;

            int halfAreaSize = (MonoJam.WINDOW_WIDTH + Size.X) / 2;
            SetX(MonoJam.WINDOW_WIDTH - (halfAreaSize * direction + halfAreaSize));

            yPos = GameController.random.Next((int)sinAmp, MonoJam.WINDOW_HEIGHT - Size.Y - (int)sinAmp / 2);
        }

        public void Update()
        {
            var previousYOffset = yOffset;

            // TODO: Based on time passed.
            yOffsetCount++;

            yOffset = sinAmp * (float)Math.Sin(yOffsetCount * sinPer);
            yOffsetDiff = yOffset - previousYOffset;

            MoveBy(new Vector2(thrust, 0));
            SetY(yPos + yOffset);
        }

        public void Damage(int amount)
        {
            totalHealth -= amount;
        }
    }
}
