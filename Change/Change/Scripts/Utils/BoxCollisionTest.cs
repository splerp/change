using Microsoft.Xna.Framework;
using System;

namespace MonoJam.Utils
{
    public class BoxCollisionTest
    {
        public static bool IntersectAABB(Rectangle r1, Rectangle r2)
        {
            var r1Pos = r1.Center;
            var r2Pos = r2.Center;
            var r1Half = r1.Size / new Point(2);
            var r2Half = r2.Size / new Point(2);

            var dx = r2Pos.X - r1Pos.X;
            var px = (r2Half.X + r1Half.X) - Math.Abs(dx);
            var dy = r2Pos.Y - r1Pos.Y;
            var py = (r2Half.Y + r1Half.Y) - Math.Abs(dy);

            return !(px <= 0 || py <= 0);
        }
    }
}
