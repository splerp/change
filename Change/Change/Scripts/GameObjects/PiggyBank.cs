using Microsoft.Xna.Framework;
using MonoJam.Controllers;
using System;

namespace MonoJam.GameObjects
{
    public class PiggyBank : Enemy
    {
        public const int WIDTH = 18;
        public const int HEIGHT = 12;

        public override Point Size => new Point(WIDTH, HEIGHT);
        public override int MaxHealth => 18000;
        public override int CoinsOnDeath => 5000;

        public float yOffsetCount;
        public float yPos;

        public float sinAmp = 3f;
        public float sinPer = 0.12f;

        public PiggyBank()
        {
            yOffsetCount = GameController.random.Next(1, 1000);

            thrust = 0.2f;
            direction = GameController.random.Next(1, 3) == 1 ? 1 : -1;
            thrust *= direction;

            int halfAreaSize = (MonoJam.PLAYABLE_AREA_WIDTH + Size.X) / 2;
            SetX(MonoJam.PLAYABLE_AREA_WIDTH - (halfAreaSize * direction + halfAreaSize));

            yPos = GameController.random.Next((int)sinAmp, MonoJam.PLAYABLE_AREA_HEIGHT - Size.Y - (int)sinAmp / 2);
            
            SoundController.Play(Sound.Oink);
        }

        public override void Update()
        {
            var previousYOffset = yOffset;

            // TODO: Based on time passed.
            yOffsetCount++;

            yOffset = sinAmp * (float)Math.Sin(yOffsetCount * sinPer);
            yOffsetDiff = yOffset - previousYOffset;

            MoveBy(new Vector2(thrust, 0));
            SetY(yPos + yOffset);
        }

        public override void OnDeath()
        {
            SoundController.Play(Sound.PigDeath);
        }
    }
}
