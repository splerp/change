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
            Point coinPos = CollisionRect.Location;
            int arrayLoc = coinPos.Y * MonoJam.PLAYABLE_AREA_WIDTH + coinPos.X;
            int finalIndex = MonoJam.PLAYABLE_AREA_WIDTH * MonoJam.PLAYABLE_AREA_HEIGHT;

            // Start exactly one line below.
            var startCheck = arrayLoc + MonoJam.PLAYABLE_AREA_WIDTH;

            if (startCheck >= finalIndex)
            {
                return true;
            }
            
            // Landed on a coin.
            if (coinData[startCheck] == 1)
            {
                const int MOVE_LEFT = 0;
                const int STAY_HERE = 1;
                const int MOVE_RIGHT = 2;

                // Choose whether to land perfectly, or slide to the sides.
                switch(GameController.random.Next(0, 3))
                {
                    case MOVE_LEFT:
                        // Move to the left?
                        if (coinPos.X > 0)
                        {
                            if (coinData[startCheck - 1] == 0)
                            {
                                MoveBy(new Vector2(-1, 0));
                                return false;
                            }
                        }
                        return true;
                    case MOVE_RIGHT:
                        // Move to the left?
                        if (coinPos.X < MonoJam.PLAYABLE_AREA_WIDTH - 1)
                        {
                            if (coinData[startCheck + 1] == 0)
                            {
                                MoveBy(new Vector2(1, 0));
                                return false;
                            }
                        }
                        return true;
                    case STAY_HERE:
                        return true;
                }
            }

            return false;
        }
    }
}
