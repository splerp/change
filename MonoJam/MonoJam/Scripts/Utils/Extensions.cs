//
// Perlin noise generator for Unity
// Keijiro Takahashi, 2013, 2015
// https://github.com/keijiro/PerlinNoise
//
// Based on the original implementation by Ken Perlin
// http://mrl.nyu.edu/~perlin/noise/
//

using MonoJam.Controllers;
using System;
using System.Collections.Generic;

namespace MonoJam.Utils
{
    public static class CollectionExtensions
    {
        public static T RandomElement<T>(this T[] coll)
            where T : class
        {
            if (coll.Length < 1)
            {
                return null;
            }

            return coll[GameController.random.Next(0, coll.Length)];
        }

        public static T RandomElement<T>(this List<T> coll)
            where T : class
        {
            if (coll.Count < 1)
            {
                return null;
            }

            return coll[GameController.random.Next(0, coll.Count)];
        }
    }
}