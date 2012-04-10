using System;
using System.Collections.Generic;

using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Common.CommandTrees.Internal;
using System.Data.Entity.Core.Common.Utils;
using System.Linq;

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Represents a DML operation expressed as a canonical command tree
    /// </summary>
    public abstract class DbModificationCommandTree : DbCommandTree
    {
        private readonly DbExpressionBinding _target;
        private System.Collections.ObjectModel.ReadOnlyCollection<DbParameterReferenceExpression> _parameters;

        internal DbModificationCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target)
            : base(metadata, dataSpace)
        {
            EntityUtil.CheckArgumentNull(target, "target");

            this._target = target;
        }

        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> that specifies the target table for the DML operation.
        /// </summary>
        public DbExpressionBinding Target
        {
            get
            {
                return _target;
            }
        }

        /// <summary>
        /// Returns true if this modification command returns a reader (for instance, to return server generated values)
        /// </summary>
        internal abstract bool HasReader
        {
            get;
        }

        internal override IEnumerable<KeyValuePair<string, TypeUsage>> GetParameters()
        {
            if (this._parameters == null)
            {
                this._parameters = ParameterRetriever.GetParameters(this);
            }
            return this._parameters.Select(p => new KeyValuePair<string, TypeUsage>(p.ParameterName, p.ResultType));
        }

        internal override void DumpStructure(ExpressionDumper dumper)
        {
            if (this.Target != null)
            {
                dumper.Dump(this.Target, "Target");
            }
        }
    }
}