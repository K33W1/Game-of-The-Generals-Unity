using System.Collections.Generic;
using UnityEngine;

namespace Extensions
{
    public static class IListExtensions
    {
        public static void Shuffle<T>(this IList<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int j = Random.Range(0, i + 1);
                T value = list[j];
                list[j] = list[i];
                list[i] = value;
            }
        }
    }
}
