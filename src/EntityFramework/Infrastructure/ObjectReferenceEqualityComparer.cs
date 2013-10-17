// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Compares objects using reference equality.
    /// </summary>
    [Serializable]
    public sealed class ObjectReferenceEqualityComparer : IEqualityComparer<object>
    {
        private static readonly ObjectReferenceEqualityComparer _default = new ObjectReferenceEqualityComparer();

        /// <summary>
        /// Gets the default instance.
        /// </summary>
        public static ObjectReferenceEqualityComparer Default
        {
            get { return _default; }
        }

        bool IEqualityComparer<object>.Equals(object x, object y)
        {
            return ReferenceEquals(x, y);
        }

        int IEqualityComparer<object>.GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}
