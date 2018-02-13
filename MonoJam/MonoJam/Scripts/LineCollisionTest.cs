using Microsoft.Xna.Framework;
using System;

namespace MonoJam
{
    class LineCollisionTest
    {
        // https://tavianator.com/fast-branchless-raybounding-box-intersections/
        public static bool IntersectRay(Rectangle r, Point rayStart, Point rayEnd)
        {
            var dir = (rayEnd - rayStart).ToVector2();
            dir.Normalize();

            var min = r.Location;
            var max = r.Location + r.Size;

            double tx1 = (min.X - rayStart.X) / dir.X;
            double tx2 = (max.X - rayStart.X) / dir.X;

            double tmin = Math.Min(tx1, tx2);
            double tmax = Math.Max(tx1, tx2);

            double ty1 = (min.Y - rayStart.Y) / dir.Y;
            double ty2 = (max.Y - rayStart.Y) / dir.Y;

            tmin = Math.Max(tmin, Math.Min(ty1, ty2));
            tmax = Math.Min(tmax, Math.Max(ty1, ty2));

            Console.WriteLine($"tmin {tmin}; tmax {tmax}");
            return tmax >= tmin;
        }

        // https://noonat.github.io/intersect/#aabb-vs-segment
        public static bool IntersectSegment(Rectangle box, Point pos, Point delta)
        {
            var boxPos = box.Center.ToVector2();
            var half = box.Size.ToVector2() / new Vector2(2);

            double scaleX = 1.0 / delta.X;
            double scaleY = 1.0 / delta.Y;
            double signX = Math.Sign(scaleX);
            double signY = Math.Sign(scaleY);
            double nearTimeX = (boxPos.X - signX * half.X - pos.X) * scaleX;
            double nearTimeY = (boxPos.Y - signY * half.Y - pos.Y) * scaleY;
            double farTimeX = (boxPos.X + signX * half.X - pos.X) * scaleX;
            double farTimeY = (boxPos.Y + signY * half.Y - pos.Y) * scaleY;

            if (nearTimeX > farTimeY || nearTimeY > farTimeX)
            {
                return false;
            }

            double nearTime = nearTimeX > nearTimeY ? nearTimeX : nearTimeY;
            double farTime = farTimeX < farTimeY ? farTimeX : farTimeY;

            if (nearTime >= 1 || farTime <= 0)
            {
                return false;
            }

            return true;
        }
    }
}
