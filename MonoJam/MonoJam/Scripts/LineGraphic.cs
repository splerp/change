using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace MonoJam
{
    public class LineGraphic
    {
        // https://stackoverflow.com/a/11683720
        public static Color[] CreateLine(int x, int y, int x2, int y2, Color lineColour)
        {
            Color[] lineData = Enumerable.Repeat(Color.Transparent, MonoJam.WINDOW_WIDTH * MonoJam.WINDOW_HEIGHT).ToArray();

            int w = x2 - x;
            int h = y2 - y;
            int dx1 = 0, dy1 = 0, dx2 = 0, dy2 = 0;
            if (w < 0) dx1 = -1; else if (w > 0) dx1 = 1;
            if (h < 0) dy1 = -1; else if (h > 0) dy1 = 1;
            if (w < 0) dx2 = -1; else if (w > 0) dx2 = 1;
            int longest = Math.Abs(w);
            int shortest = Math.Abs(h);
            if (!(longest > shortest))
            {
                longest = Math.Abs(h);
                shortest = Math.Abs(w);
                if (h < 0) dy2 = -1; else if (h > 0) dy2 = 1;
                dx2 = 0;
            }
            int numerator = longest >> 1;
            for (int i = 0; i <= longest; i++)
            {
                lineData[x + y * MonoJam.WINDOW_WIDTH] = lineColour;
                numerator += shortest;
                if (!(numerator < longest))
                {
                    numerator -= longest;
                    x += dx1;
                    y += dy1;
                }
                else
                {
                    x += dx2;
                    y += dy2;
                }
            }

            return lineData;
        }
    }
}
