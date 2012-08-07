// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Collections.Generic;

    /// <summary>
    ///     Helper class that extends Tuple to give the Item1 and Item2 properties more meaningful names.
    /// </summary>
    internal class DbContextTypesInitializersPair : Tuple<Dictionary<Type, List<string>>, Action<DbContext>>
    {
        #region Constructor

        /// <summary>
        ///     Creates a new pair of the given set of entity types and DbSet initializer delegate.
        /// </summary>
        public DbContextTypesInitializersPair(
            Dictionary<Type, List<string>> entityTypeToPropertyNameMap, Action<DbContext> setsInitializer)
            : base(entityTypeToPropertyNameMap, setsInitializer)
        {
        }

        #endregion

        #region Properties

        /// <summary>
        ///     The entity types part of the pair.
        /// </summary>
        public Dictionary<Type, List<string>> EntityTypeToPropertyNameMap
        {
            get { return Item1; }
        }

        /// <summary>
        ///     The DbSet properties initializer part of the pair.
        /// </summary>
        public Action<DbContext> SetsInitializer
        {
            get { return Item2; }
        }

        #endregion
    }
}
