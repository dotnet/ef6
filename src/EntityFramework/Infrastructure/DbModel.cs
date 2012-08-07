// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Represents an Entity Data Model (EDM) created by the <see cref="DbModelBuilder" />.
    ///     The Compile method can be used to go from this EDM representation to a <see cref="DbCompiledModel" />
    ///     which is a compiled snapshot of the model suitable for caching and creation of
    ///     <see cref="DbContext" /> or <see cref="T:System.Data.Objects.ObjectContext" /> instances.
    /// </summary>
    public class DbModel
    {
        #region Fields and constructore

        private readonly DbDatabaseMapping _databaseMapping;
        private readonly DbModelBuilder _cachedModelBuilder;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DbModel" /> class.
        /// </summary>
        internal DbModel(DbDatabaseMapping databaseMapping, DbModelBuilder modelBuilder)
        {
            Contract.Requires(databaseMapping != null);
            Contract.Requires(modelBuilder != null);

            _databaseMapping = databaseMapping;
            _cachedModelBuilder = modelBuilder;
        }

        #endregion

        #region Internal properties

        /// <summary>
        ///     A snapshot of the <see cref="DbModelBuilder" /> that was used to create this compiled model.
        /// </summary>
        internal DbModelBuilder CachedModelBuilder
        {
            get { return _cachedModelBuilder; }
        }

        internal DbDatabaseMapping DatabaseMapping
        {
            get { return _databaseMapping; }
        }

        #endregion

        #region Compile

        /// <summary>
        ///     Creates a <see cref="DbCompiledModel" /> for this mode which is a compiled snapshot
        ///     suitable for caching and creation of <see cref="DbContext" /> instances.
        /// </summary>
        /// <returns> The compiled model. </returns>
        public DbCompiledModel Compile()
        {
            return new DbCompiledModel(this);
        }

        #endregion
    }
}
