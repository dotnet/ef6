// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>Supports standard aggregate functions, such as MIN, MAX, AVG, SUM, and so on. This class cannot be inherited.</summary>
    public sealed class DbFunctionAggregate : DbAggregate
    {
        private readonly bool _distinct;
        private readonly EdmFunction _aggregateFunction;

        internal DbFunctionAggregate(TypeUsage resultType, DbExpressionList arguments, EdmFunction function, bool isDistinct)
            : base(resultType, arguments)
        {
            DebugCheck.NotNull(function);

            _aggregateFunction = function;
            _distinct = isDistinct;
        }

        /// <summary>Gets a value indicating whether this aggregate is a distinct aggregate.</summary>
        /// <returns>true if the aggregate is a distinct aggregate; otherwise, false. </returns>
        public bool Distinct
        {
            get { return _distinct; }
        }

        /// <summary>Gets the method metadata that specifies the aggregate function to invoke.</summary>
        /// <returns>The method metadata that specifies the aggregate function to invoke.</returns>
        public EdmFunction Function
        {
            get { return _aggregateFunction; }
        }
    }
}
