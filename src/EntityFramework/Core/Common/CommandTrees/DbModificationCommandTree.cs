namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    /// Represents a DML operation expressed as a canonical command tree
    /// </summary>
    public abstract class DbModificationCommandTree : DbCommandTree
    {
        private readonly DbExpressionBinding _target;
        private ReadOnlyCollection<DbParameterReferenceExpression> _parameters;

        internal DbModificationCommandTree()
        {
        }

        internal DbModificationCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target)
            : base(metadata, dataSpace)
        {
            Contract.Requires(target != null);

            _target = target;
        }

        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> that specifies the target table for the DML operation.
        /// </summary>
        public DbExpressionBinding Target
        {
            get { return _target; }
        }

        /// <summary>
        /// Returns true if this modification command returns a reader (for instance, to return server generated values)
        /// </summary>
        internal abstract bool HasReader { get; }

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
            if (Target != null)
            {
                dumper.Dump(Target, "Target");
            }
        }
    }
}
