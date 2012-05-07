namespace System.Data.Entity.Core.Common.Utils
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Extends IComparer support to the (non-IComparable) byte[] type, based on by-value comparison.
    /// </summary>
    internal class ByValueComparer : IComparer
    {
        internal static readonly IComparer Default = new ByValueComparer(Comparer<object>.Default);

        private readonly IComparer nonByValueComparer;

        private ByValueComparer(IComparer comparer)
        {
            Debug.Assert(comparer != null, "Non-ByValue comparer cannot be null");
            nonByValueComparer = comparer;
        }

        int IComparer.Compare(object x, object y)
        {
            if (ReferenceEquals(x, y))
            {
                return 0;
            }

            //We can convert DBNulls to nulls for the purposes of comparison.
            Debug.Assert(
                !((ReferenceEquals(x, DBNull.Value)) && (ReferenceEquals(y, DBNull.Value))),
                "object.ReferenceEquals should catch the case when both values are dbnull");
            if (ReferenceEquals(x, DBNull.Value))
            {
                x = null;
            }
            if (ReferenceEquals(y, DBNull.Value))
            {
                y = null;
            }

            if (x != null
                && y != null)
            {
                var xAsBytes = x as byte[];
                var yAsBytes = y as byte[];
                if (xAsBytes != null
                    && yAsBytes != null)
                {
                    var result = xAsBytes.Length - yAsBytes.Length;
                    if (result == 0)
                    {
                        var idx = 0;
                        while (result == 0
                               && idx < xAsBytes.Length)
                        {
                            var xVal = xAsBytes[idx];
                            var yVal = yAsBytes[idx];
                            if (xVal != yVal)
                            {
                                result = xVal - yVal;
                            }
                            idx++;
                        }
                    }
                    return result;
                }
            }

            return nonByValueComparer.Compare(x, y);
        }
    }
}