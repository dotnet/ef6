namespace System.Data.Entity
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    public static class EnumeratorExtensions
    {
        public static List<T> ToList<T>(this IEnumerator<T> enumerator)
        {
            Contract.Requires(enumerator != null);

            List<T> resultList = new List<T>();

            while (enumerator.MoveNext())
            {
                resultList.Add(enumerator.Current);
            }

            return resultList;
        }
    }
}
