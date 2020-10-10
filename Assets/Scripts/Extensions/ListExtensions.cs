using System.Collections.Generic;

namespace Extensions
{
    public static class ListExtensions
    {
        public static List<T> Copy<T>(this List<T> list) where T : ICopyable<T>
        {
            List<T> newList = new List<T>(list.Count);
            for (int i = 0; i < list.Count; i++)
                newList[i] = list[i].Copy();
            return newList;
        }
    }
}
