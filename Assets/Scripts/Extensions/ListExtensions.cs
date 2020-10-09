using System;
using System.Collections.Generic;

public static class ListExtensions
{
    private static readonly Random random = new Random();
    
    public static void Shuffle<T>(this List<T> list)
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
