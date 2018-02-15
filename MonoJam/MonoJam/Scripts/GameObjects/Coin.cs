using Microsoft.Xna.Framework;
using MonoJam.Controllers;
using System;

namespace MonoJam.GameObjects
{
    public class Coin : GameObject
    {
        public Rectangle CollisionRect => new Rectangle(Position.ToPoint(), new Point(1, 1));

        public int fallBy;

        public Coin()
        {
            fallBy = GameController.random.Next(1, 5);
            SetY(-1);
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
            
            if (coinData[startCheck] == 1)
            {
                return true;
            }

            return false;
        }
    }
}
