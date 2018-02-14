using Microsoft.Xna.Framework;
using MonoJam.Controllers;
using System;

namespace MonoJam.GameObjects
{
    public class Coin : GameObject, ICollisionObject
    {
        public const int COIN_WIDTH = 1;

        public Point Size => new Point(COIN_WIDTH, 1);
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        public int fallBy;

        public Coin()
        {
            fallBy = GameController.random.Next(1, 5);
            SetY(-Size.Y);
        }

        public bool MoveAndCheckLand(byte[] coinData)
        {
            Point coinCorner = CollisionRect.Location;
            int arrayLoc = coinCorner.Y * MonoJam.PLAYABLE_AREA_WIDTH + coinCorner.X;
            int finalIndex = MonoJam.PLAYABLE_AREA_WIDTH * MonoJam.PLAYABLE_AREA_HEIGHT;

            // Start exactly one line below.
            var startCheck = arrayLoc + MonoJam.PLAYABLE_AREA_WIDTH;

            if (startCheck >= finalIndex)
            {
                return true;
            }
            
            // End WIDTH to the right.
            var endCheck = Math.Min(startCheck + COIN_WIDTH, finalIndex);

            for(int i = startCheck; i < endCheck; i++)
            {
                if(coinData[i] == 1)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
