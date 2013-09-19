// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using ReadOnlyModificationClauses =
    System.Collections.ObjectModel.ReadOnlyCollection<System.Data.Entity.Core.Common.CommandTrees.DbModificationClause>;

// System.Data.Common.ReadOnlyCollection conflicts

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>Represents a single-row update operation expressed as a command tree. This class cannot be inherited.  </summary>
    /// <remarks>
    /// Represents a single-row update operation expressed as a canonical command tree.
    /// When the <see cref="Returning" /> property is set, the command returns a reader; otherwise,
    /// it returns a scalar indicating the number of rows affected.
    /// </remarks>
    public sealed class DbUpdateCommandTree : DbModificationCommandTree
    {
        private readonly DbExpression _predicate;
        private readonly DbExpression _returning;
        private readonly ReadOnlyModificationClauses _setClauses;

        internal DbUpdateCommandTree()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DbUpdateCommandTree"/> class.
        /// </summary>
        /// <param name="metadata">The model this command will operate on.</param>
        /// <param name="dataSpace">The data space.</param>
        /// <param name="target">The target table for the data manipulation language (DML) operation.</param>
        /// <param name="predicate">A predicate used to determine which members of the target collection should be updated.</param>
        /// <param name="setClauses">The list of update set clauses that define the update operation.</param>
        /// <param name="returning">A <see cref="DbExpression"/> that specifies a projection of results to be returned, based on the modified rows.</param>
        public DbUpdateCommandTree(
            MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target, DbExpression predicate,
            ReadOnlyModificationClauses setClauses, DbExpression returning)
            : base(metadata, dataSpace, target)
        {
            DebugCheck.NotNull(predicate);
            DebugCheck.NotNull(setClauses);
            // returning is allowed to be null

            _predicate = predicate;
            _setClauses = setClauses;
            _returning = returning;
        }

        /// <summary>Gets the list of update set clauses that define the update operation.</summary>
        /// <returns>The list of update set clauses that define the update operation.</returns>
        public IList<DbModificationClause> SetClauses
        {
            get { return _setClauses; }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that specifies a projection of results to be returned, based on the modified rows.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that specifies a projection of results to be returned based, on the modified rows. null indicates that no results should be returned from this command.
        /// </returns>
        public DbExpression Returning
        {
            get { return _returning; }
        }

        /// <summary>
        /// Gets an <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that specifies the predicate used to determine which members of the target collection should be updated.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that specifies the predicate used to determine which members of the target collection should be updated.
        /// </returns>
        public DbExpression Predicate
        {
            get { return _predicate; }
        }

        /// <summary>Gets the kind of this command tree.</summary>
        /// <returns>The kind of this command tree.</returns>
        public override DbCommandTreeKind CommandTreeKind
        {
            get { return DbCommandTreeKind.Update; }
        }

        internal override bool HasReader
        {
            get { return null != Returning; }
        }

        internal override void DumpStructure(ExpressionDumper dumper)
        {
            base.DumpStructure(dumper);

            if (Predicate != null)
            {
                dumper.Dump(Predicate, "Predicate");
            }

            dumper.Begin("SetClauses", null);
            foreach (var clause in SetClauses)
            {
                if (null != clause)
                {
                    clause.DumpStructure(dumper);
                }
            }
            dumper.End("SetClauses");

            dumper.Dump(Returning, "Returning");
        }

        internal override string PrintTree(ExpressionPrinter printer)
        {
            return printer.Print(this);
        }
    }
}
