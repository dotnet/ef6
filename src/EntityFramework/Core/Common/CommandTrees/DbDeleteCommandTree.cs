namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents a single row delete operation expressed as a canonical command tree.
    /// </summary>
    public class DbDeleteCommandTree : DbModificationCommandTree
    {
        private readonly DbExpression _predicate;

        internal DbDeleteCommandTree()
        {
        }

        internal DbDeleteCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target, DbExpression predicate)
            : base(metadata, dataSpace, target)
        {
            Contract.Requires(predicate != null);

            _predicate = predicate;
        }

        /// <summary>
        /// Gets an <see cref="DbExpression"/> that specifies the predicate used to determine which members of the target collection should be deleted.
        /// </summary>
        /// <remarks>
        /// The predicate can include only the following elements:
        /// <list>
        /// <item>Equality expression</item>
        /// <item>Constant expression</item>
        /// <item>IsNull expression</item>
        /// <item>Property expression</item>
        /// <item>Reference expression to the target</item>
        /// <item>And expression</item>
        /// <item>Or expression</item>
        /// <item>Not expression</item>
        /// </list>
        /// </remarks>
        public DbExpression Predicate
        {
            get { return _predicate; }
        }

        public override DbCommandTreeKind CommandTreeKind
        {
            get { return DbCommandTreeKind.Delete; }
        }

        internal override bool HasReader
        {
            get
            {
                // a delete command never returns server-gen values, and
                // therefore never returns a reader
                return false;
            }
        }

        internal override void DumpStructure(ExpressionDumper dumper)
        {
            base.DumpStructure(dumper);

            if (Predicate != null)
            {
                dumper.Dump(Predicate, "Predicate");
            }
        }

        internal override string PrintTree(ExpressionPrinter printer)
        {
            return printer.Print(this);
        }
    }
}
