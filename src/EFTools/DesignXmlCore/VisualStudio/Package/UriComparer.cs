// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Collections.Generic;

    internal class UriComparer : IEqualityComparer<Uri>
    {
        private UriComparer()
        {
        }

        private static readonly UriComparer _ordinalIgnoreCase = new UriComparer();

        internal static UriComparer OrdinalIgnoreCase
        {
            get { return _ordinalIgnoreCase; }
        }

        #region IEqualityComparer<Uri> Members

        public bool Equals(Uri uri1, Uri uri2)
        {
            return 0 == Uri.Compare(uri1, uri2, UriComponents.AbsoluteUri, UriFormat.UriEscaped, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode(Uri uri)
        {
            return StringComparer.OrdinalIgnoreCase.GetHashCode(
                uri.GetComponents(UriComponents.AbsoluteUri, UriFormat.UriEscaped));
        }

        #endregion
    }
}
