using ReadOnlyModificationClauses =
    System.Collections.ObjectModel.ReadOnlyCollection<System.Data.Entity.Core.Common.CommandTrees.DbModificationClause>;

// System.Data.Common.ReadOnlyCollection conflicts

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents a single row insert operation expressed as a canonical command tree.
    /// When the <see cref="Returning"/> property is set, the command returns a reader; otherwise,
    /// it returns a scalar value indicating the number of rows affected.
    /// </summary>
    public class DbInsertCommandTree : DbModificationCommandTree
    {
        private readonly ReadOnlyModificationClauses _setClauses;
        private readonly DbExpression _returning;

        internal DbInsertCommandTree()
        {
        }

        internal DbInsertCommandTree(
            MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target, ReadOnlyModificationClauses setClauses,
            DbExpression returning)
            : base(metadata, dataSpace, target)
        {
            Contract.Requires(setClauses != null);
            // returning may be null

            _setClauses = setClauses;
            _returning = returning;
        }

        /// <summary>
        /// Gets set clauses determining values of columns in the inserted row.
        /// </summary>
        public IList<DbModificationClause> SetClauses
        {
            get { return _setClauses; }
        }

        /// <summary>
        /// Gets an <see cref="DbExpression"/> that specifies a projection of results to be returned based on the modified rows.
        /// If null, indicates no results should be returned from this command.
        /// </summary>
        /// <remarks>
        /// The returning projection includes only the following elements:
        /// <list>
        /// <item>NewInstance expression</item>
        /// <item>Property expression</item>
        /// </list>
        /// </remarks>
        public DbExpression Returning
        {
            get { return _returning; }
        }

        public override DbCommandTreeKind CommandTreeKind
        {
            get { return DbCommandTreeKind.Insert; }
        }

        internal override bool HasReader
        {
            get { return null != Returning; }
        }

        internal override void DumpStructure(ExpressionDumper dumper)
        {
            base.DumpStructure(dumper);

            dumper.Begin("SetClauses");
            foreach (var clause in SetClauses)
            {
                if (null != clause)
                {
                    clause.DumpStructure(dumper);
                }
            }
            dumper.End("SetClauses");

            if (null != Returning)
            {
                dumper.Dump(Returning, "Returning");
            }
        }

        internal override string PrintTree(ExpressionPrinter printer)
        {
            return printer.Print(this);
        }
    }
}
