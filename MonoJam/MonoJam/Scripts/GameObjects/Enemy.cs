using Microsoft.Xna.Framework;
using MonoJam.Controllers;
using System;

namespace MonoJam.GameObjects
{
    public class Enemy : GameObject, ICollisionObject, IHurtable
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
        
        public bool ReadyToRemove => Position.X + Size.X < 0 || Position.X > MonoJam.PLAYABLE_AREA_WIDTH;

        public int MaxHealth => 1000;
        public int CurrentHealth { get; set; }
        public bool IsDead => CurrentHealth <= 0;

        public Enemy()
        {
            CurrentHealth = MaxHealth;

            yOffsetCount = GameController.random.Next(1, 1000);

            direction = GameController.random.Next(1, 3) == 1 ? 1 : -1;
            thrust *= direction;

            int halfAreaSize = (MonoJam.PLAYABLE_AREA_WIDTH + Size.X) / 2;
            SetX(MonoJam.PLAYABLE_AREA_WIDTH - (halfAreaSize * direction + halfAreaSize));

            yPos = GameController.random.Next((int)sinAmp, MonoJam.PLAYABLE_AREA_HEIGHT - Size.Y - (int)sinAmp / 2);
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
            CurrentHealth -= amount;
        }
    }
}
