using Splerp.Change.Controllers;
using System;
using System.Collections.Generic;

namespace Splerp.Change.Utils
{
    public static class CollectionExtensions
    {
        // For a given array, pick a random entry.
        public static T RandomElement<T>(this T[] coll)
            where T : class
        {
            if (coll.Length < 1)
            {
                return null;
            }

            return coll[GameController.random.Next(0, coll.Length)];
        }

        // For a given list, pick a random entry.
        public static T RandomElement<T>(this List<T> coll)
            where T : class
        {
            if (coll.Count < 1)
            {
                return null;
            }

            return coll[GameController.random.Next(0, coll.Count)];
        }

        // For a given enum, pick a random entry.
        public static T RandomEnumElement<T>(this T enumVal)
            where T : struct, IConvertible, IComparable, IFormattable
        {
            var values = Enum.GetValues(typeof(T));
            return (T)values.GetValue(GameController.random.Next(values.Length));
        }
    }
}