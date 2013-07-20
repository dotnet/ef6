// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure.MappingViews
{
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Represents a mapping view.
    /// </summary>
    public class DbMappingView
    {
        private readonly string _entitySql;

        /// <summary>
        ///     Creates a <see cref="DbMappingView"/> instance having the specified entity SQL.
        /// </summary>
        /// <param name="entitySql">A string that specifies the entity SQL.</param>
        public DbMappingView(string entitySql)
        {
            Check.NotEmpty(entitySql, "entitySql");

            _entitySql = entitySql;
        }

        /// <summary>
        ///     Gets the entity SQL.
        /// </summary>
        public string EntitySql
        {
            get { return _entitySql; }
        }
    }
}
