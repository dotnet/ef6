// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;

    /// <summary>Represents a query operation expressed as a command tree. This class cannot be inherited.  </summary>
    public sealed class DbQueryCommandTree : DbCommandTree
    {
        // Query expression
        private readonly DbExpression _query;

        // Parameter information (will be retrieved from the query expression of the command tree during construction)
        private ReadOnlyCollection<DbParameterReferenceExpression> _parameters;

        /// <summary>
        /// Constructs a new DbQueryCommandTree that uses the specified metadata workspace.
        /// </summary>
        /// <param name="metadata"> The metadata workspace that the command tree should use. </param>
        /// <param name="dataSpace"> The logical 'space' that metadata in the expressions used in this command tree must belong to. </param>
        /// <param name="query">
        /// A <see cref="DbExpression" /> that defines the logic of the query.
        /// </param>
        /// <param name="validate"> When set to false the validation of the tree is turned off. </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="metadata" />
        /// or
        /// <paramref name="query" />
        /// is null
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="dataSpace" />
        /// does not represent a valid data space
        /// </exception>
        public DbQueryCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpression query, bool validate)
            : base(metadata, dataSpace)
        {
            // Ensure the query expression is non-null
            Check.NotNull(query, "query");

            if (validate)
            {
                // Use the valid workspace and data space to validate the query expression
                var validator = new DbExpressionValidator(metadata, dataSpace);
                validator.ValidateExpression(query, "query");

                _parameters = new ReadOnlyCollection<DbParameterReferenceExpression>(
                    validator.Parameters.Select(paramInfo => paramInfo.Value).ToList());
            }
            _query = query;
        }

        /// <summary>
        /// Constructs a new DbQueryCommandTree that uses the specified metadata workspace.
        /// </summary>
        /// <param name="metadata"> The metadata workspace that the command tree should use. </param>
        /// <param name="dataSpace"> The logical 'space' that metadata in the expressions used in this command tree must belong to. </param>
        /// <param name="query">
        /// A <see cref="DbExpression" /> that defines the logic of the query.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="metadata" />
        /// or
        /// <paramref name="query" />
        /// is null
        /// </exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="dataSpace" />
        /// does not represent a valid data space
        /// </exception>
        public DbQueryCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpression query)
            : this(metadata, dataSpace, query, true)
        {
        }

        /// <summary>
        /// Gets an <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the logic of the query operation.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the logic of the query operation.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">The expression is associated with a different command tree.</exception>
        public DbExpression Query
        {
            get { return _query; }
        }

        /// <summary>Gets the kind of this command tree.</summary>
        /// <returns>The kind of this command tree.</returns>
        public override DbCommandTreeKind CommandTreeKind
        {
            get { return DbCommandTreeKind.Query; }
        }

        internal override IEnumerable<KeyValuePair<string, TypeUsage>> GetParameters()
        {
            if (_parameters == null)
            {
                _parameters = ParameterRetriever.GetParameters(this);
            }
            return _parameters.Select(p => new KeyValuePair<string, TypeUsage>(p.ParameterName, p.ResultType));
        }

        internal override void DumpStructure(ExpressionDumper dumper)
        {
            if (Query != null)
            {
                dumper.Dump(Query, "Query");
            }
        }

        internal override string PrintTree(ExpressionPrinter printer)
        {
            return printer.Print(this);
        }

        internal static DbQueryCommandTree FromValidExpression(MetadataWorkspace metadata, DataSpace dataSpace, DbExpression query)
        {
#if DEBUG
            return new DbQueryCommandTree(metadata, dataSpace, query);
#else
            return new DbQueryCommandTree(metadata, dataSpace, query, false);
#endif
        }
    }
}
