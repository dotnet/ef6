// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Eventing
{
    using System.Collections.Generic;

    /// <summary>
    ///     This class is used to sort EfiChanges using stable sort algorithm (i.e. one that is preserving order of equal values)
    ///     It remembers original position of EfiChange so the compare method can decide which should come first.
    /// </summary>
    internal class EfiChangeStableSortItem
    {
        private readonly EfiChange _change;
        private readonly int _position;

        public EfiChangeStableSortItem(EfiChange change, int position)
        {
            _change = change;
            _position = position;
        }

        public EfiChange EfiChange
        {
            get { return _change; }
        }

        public int Position
        {
            get { return _position; }
        }
    }

    internal abstract class EfiChangeComparer : IComparer<EfiChangeStableSortItem>
    {
        /// <summary>
        ///     Sort changes so that Deletes are first, creates are second and updates are third.
        ///     For example: Sort updates to process EntityType first, then Associaiton, then others.
        ///     Sort is stable (i.e. it is preserving order of items that have equal values).
        ///     This is acomplished by comparing original position of items for which GetVal() returned same value.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare(EfiChangeStableSortItem x, EfiChangeStableSortItem y)
        {
            var xval = GetVal(x.EfiChange);
            var yval = GetVal(y.EfiChange);
            if (xval > yval)
            {
                return 1;
            }
            else if (xval < yval)
            {
                return -1;
            }
            else
            {
                if (x.Position > y.Position)
                {
                    return 1;
                }
                else if (x.Position < y.Position)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }

        protected abstract int GetVal(EfiChange change);
    }
}
