// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics;

    internal class SafeLink<TParent>
        where TParent : class
    {
        private TParent _value;

        public TParent Value
        {
            get { return _value; }
        }

        internal static IEnumerable<TChild> BindChildren<TChild>(
            TParent parent, Func<TChild, SafeLink<TParent>> getLink, IEnumerable<TChild> children)
        {
            foreach (var child in children)
            {
                BindChild(parent, getLink, child);
            }
            return children;
        }

        internal static TChild BindChild<TChild>(TParent parent, Func<TChild, SafeLink<TParent>> getLink, TChild child)
        {
            var link = getLink(child);

            Debug.Assert(link._value == null || link._value == parent, "don't try to hook up the same child to a different parent");
            // this is the good stuff.. 
            // only this method can actually make the link since _value is a private
            link._value = parent;

            return child;
        }
    }
}
