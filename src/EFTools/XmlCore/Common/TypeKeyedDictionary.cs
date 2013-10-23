// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Common
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    ///     This is the dictionary to use if the key is a type.  This dictionary understands
    ///     Embedded Interop Types (aka "noPIA").
    /// </summary>
    /// <typeparam name="T">The type of the value</typeparam>
    internal class TypeKeyedDictionary<T> : Dictionary<Type, T>
    {
        private class EmbeddedTypeAwareTypeComparer : IEqualityComparer<Type>
        {
            #region IEqualityComparer<Type> Members

            public bool Equals(Type x, Type y)
            {
                return x.GUID == y.GUID;
            }

            public int GetHashCode(Type obj)
            {
                return obj.GUID.GetHashCode();
            }

            #endregion
        }

        private static IEqualityComparer<Type> typeEqualityComparer = new EmbeddedTypeAwareTypeComparer();

        public TypeKeyedDictionary()
            : base(typeEqualityComparer)
        {
        }
    }
}
