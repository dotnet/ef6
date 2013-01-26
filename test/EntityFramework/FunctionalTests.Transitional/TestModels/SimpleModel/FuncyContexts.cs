// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace DaFunc
{
    using System.Data.Entity;

    /// <summary>
    ///     A normal type with an intentionally short name--see above.
    /// </summary>
    public class NT
    {
    }

    /// <summary>
    ///     A generic type with an intentionally short name--see above.
    /// </summary>
    public class GT<T1, T2>
    {
        /// <summary>
        ///     A context type nested in a generic type.
        /// </summary>
        public class Funcy : DbContext
        {
        }

        /// <summary>
        ///     A generic context type nested in a generic type.
        /// </summary>
        public class GenericFuncy<T3, T4> : DbContext
        {
        }
    }

    /// <summary>
    ///     A generic context type.
    /// </summary>
    public class GenericFuncy<T1, T2> : DbContext
    {
    }
}
