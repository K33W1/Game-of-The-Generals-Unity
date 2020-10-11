using System.Collections.Generic;

namespace Extensions
{
    public static class ListExtensions
    {
        public static List<T> DeepCopy<T>(this List<T> list) where T : IDeepCopyable<T>
        {
            List<T> newList = new List<T>(list.Capacity);
            for (int i = 0; i < list.Count; i++)
                newList.Add(list[i].DeepCopy());
            return newList;
        }
    }
}
