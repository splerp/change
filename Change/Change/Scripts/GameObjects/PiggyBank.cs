using Microsoft.Xna.Framework;
using Splerp.Change.Controllers;
using System;

namespace Splerp.Change.GameObjects
{
    public sealed class PiggyBank : Enemy
    {
        public const int WIDTH = 18;
        public const int HEIGHT = 12;

        // Set Enemy-related properties.
        public override Point Size => new Point(WIDTH, HEIGHT);
        public override int MaxHealth => 18000;
        public override int CoinsOnDeath => 5000;

        // Used for Y offset calculation.
        public float sinAmp = 3f;
        public float sinPer = 0.12f;

        public float yOffsetCount;
        public float yPos;

        public PiggyBank()
        {
            yOffsetCount = GameController.random.Next(1, 1000);

            Speed = new Vector2(0.2f, 0);
            Direction = GameController.random.Next(0, 2) == 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
            Speed *= (int)Direction;

            int halfAreaSize = (ChangeGame.PLAYABLE_AREA_WIDTH + Size.X) / 2;
            SetX(ChangeGame.PLAYABLE_AREA_WIDTH - (halfAreaSize * (int)Direction + halfAreaSize));

            yPos = GameController.random.Next((int)sinAmp, ChangeGame.PLAYABLE_AREA_HEIGHT - Size.Y - (int)sinAmp / 2);
            
            SoundController.Play(Sound.Oink);
        }

        public override void Update()
        {
            PreviousOffset = Offset;

            // TODO: Based on time passed.
            yOffsetCount++;

            Offset = new Vector2(0, sinAmp * (float)Math.Sin(yOffsetCount * sinPer));

            MoveBy(Speed);
            SetY(yPos + Offset.Y);
        }

        public override void OnDeath()
        {
            SoundController.Play(Sound.PigDeath);
        }
    }
}
