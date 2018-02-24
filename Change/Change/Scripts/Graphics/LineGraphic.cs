using Microsoft.Xna.Framework;
using System;
using System.Linq;

namespace Splerp.Change.Graphics
{
    public sealed class LineGraphic
    {
        // https://stackoverflow.com/a/11683720
        public static Color[] CreateLine(int x, int y, int x2, int y2, Color lineColour)
        {
            Color[] lineData = Enumerable.Repeat(Color.Transparent,
                ChangeGame.PLAYABLE_AREA_WIDTH * ChangeGame.PLAYABLE_AREA_HEIGHT).ToArray();

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
                lineData[x + y * ChangeGame.WINDOW_WIDTH] = lineColour;
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

        public static Color[] CreateLineBoundsCheck(int x, int y, int x2, int y2, Color lineColour)
        {
            Color[] lineData = Enumerable.Repeat(Color.Transparent,
                ChangeGame.PLAYABLE_AREA_WIDTH * ChangeGame.PLAYABLE_AREA_HEIGHT).ToArray();

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
                // This if check is the only difference from the above.
                if(x >= 0 && x < ChangeGame.PLAYABLE_AREA_WIDTH && y >= 0 && y < ChangeGame.PLAYABLE_AREA_HEIGHT)
                {
                    lineData[x + y * ChangeGame.PLAYABLE_AREA_WIDTH] = lineColour;
                }
                
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
