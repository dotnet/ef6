namespace System.Data.Entity
{
    using System.Collections.Generic;

    public static class ListExtensions
    {
        public static T AddAndReturn<T>(this List<T> list, T elementToAdd)
        {
            list.Add(elementToAdd);
            return elementToAdd;
        }
    }
}