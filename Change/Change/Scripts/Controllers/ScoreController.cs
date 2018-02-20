using System.Collections.Generic;

namespace MonoJam.Controllers
{
    public static class ScoreController
    {
        // Lengths in pixels.
        public static Dictionary<char, int> charLengths = new Dictionary<char, int>
        {
            { '0', 3 },
            { '1', 2 },
            { '2', 3 },
            { '3', 3 },
            { '4', 3 },
            { '5', 3 },
            { '6', 3 },
            { '7', 3 },
            { '8', 3 },
            { '9', 3 },
            { '.', 2 }
        };

        public static int OffsetOf(string numStr, int index)
        {
            // The length of the initial dollar sign graphic.
            int offset = 4;
            for(int i = 0; i < index; i++)
            {
                if(charLengths.ContainsKey(numStr[i]))
                {
                    offset += charLengths[numStr[i]];
                }
            }

            return offset;
        }

        public static string StringFor(double num)
        {
            return num.ToString("0.00");
        }

        public static int LengthOf(double num)
        {
            int length = 4;
            foreach(var c in StringFor(num))
            {
                if (charLengths.ContainsKey(c))
                {
                    length += charLengths[c];
                }
            }

            return length;
        }
    }
}
