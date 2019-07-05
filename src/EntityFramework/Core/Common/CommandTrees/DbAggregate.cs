// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>Implements the basic functionality required by aggregates in a GroupBy clause. </summary>
    public abstract class DbAggregate
    {
        private readonly DbExpressionList _args;
        private readonly TypeUsage _type;

        internal DbAggregate(TypeUsage resultType, DbExpressionList arguments)
        {
            DebugCheck.NotNull(resultType);
            DebugCheck.NotNull(arguments);
            Debug.Assert(arguments.Count >= 1, "DbAggregate requires at least one argument");

            _type = resultType;
            _args = arguments;
        }

        /// <summary>
        /// Gets the result type of this <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbAggregate" />.
        /// </summary>
        /// <returns>
        /// The result type of this <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbAggregate" />.
        /// </returns>
        public TypeUsage ResultType
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the list of expressions that define the arguments to this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbAggregate" />
        /// .
        /// </summary>
        /// <returns>
        /// The list of expressions that define the arguments to this
        /// <see
        ///     cref="T:System.Data.Entity.Core.Common.CommandTrees.DbAggregate" />
        /// .
        /// </returns>
        public IList<DbExpression> Arguments
        {
            get { return _args; }
        }
    }
}
