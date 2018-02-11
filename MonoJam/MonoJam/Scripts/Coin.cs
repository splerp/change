using Microsoft.Xna.Framework;
using System;

namespace MonoJam
{
    class Coin : GameObject, ICollisionObject
    {
        public const int COIN_WIDTH = 3;

        public Point Size => new Point(COIN_WIDTH, 1);
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), Size);

        public int fallBy;

        public Coin()
        {
            fallBy = new Random().Next(2, 5);
        }

        public bool MoveCheckLand(byte[] coinData)
        {
            Point coinCorner = CollisionRect.Location;
            int arrayLoc = coinCorner.Y * MonoJam.WINDOW_WIDTH + coinCorner.X;
            int finalIndex = MonoJam.WINDOW_WIDTH * MonoJam.WINDOW_HEIGHT;

            // Start exactly one line below.
            var startCheck = arrayLoc + MonoJam.WINDOW_WIDTH;

            if (startCheck >= finalIndex)
            {
                return true;
            }
            
            // End WIDTH to the right.
            var endCheck = Math.Min(startCheck + COIN_WIDTH, finalIndex - 1);

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
