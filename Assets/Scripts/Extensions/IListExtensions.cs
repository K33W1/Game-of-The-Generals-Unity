using System;
using System.Collections.Generic;

namespace Extensions
{
    public static class IListExtensions
    {
        private static readonly Random random = new Random();

        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = random.Next(0, i);
                T value = list[j];
                list[j] = list[i];
                list[i] = value;
            }
        }
    }
}
