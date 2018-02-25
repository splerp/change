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
        public double sinAmp = 3d;
        public double sinPer = 7.5d;

        public double yOffsetCount;
        public float yPos;

        public PiggyBank()
        {
            yOffsetCount = GameController.random.Next(1, 1000);

            Speed = new Vector2(11f, 0);
            Direction = GameController.random.Next(0, 2) == 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
            Speed *= (int)Direction;

            int halfAreaSize = (ChangeGame.PLAYABLE_AREA_WIDTH + Size.X) / 2;
            SetX(ChangeGame.PLAYABLE_AREA_WIDTH - (halfAreaSize * (int)Direction + halfAreaSize));

            yPos = GameController.random.Next((int)sinAmp, ChangeGame.PLAYABLE_AREA_HEIGHT - Size.Y - (int)sinAmp / 2);
            
            SoundController.Play(Sound.Oink);
        }

        public override void Update(GameTime gameTime)
        {
            PreviousOffset = Offset;
            
            yOffsetCount += gameTime.ElapsedGameTime.TotalSeconds;

            Offset = new Vector2(0, (float)(sinAmp * Math.Sin(yOffsetCount * sinPer)));

            MoveBy(Speed * (float)gameTime.ElapsedGameTime.TotalSeconds);
            SetY(yPos + Offset.Y);
        }

        public override void OnDeath()
        {
            SoundController.Play(Sound.PigDeath);
        }
    }
}
