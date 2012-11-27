// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.ELinq
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Utilities;
    using System.Linq.Expressions;

    /// <summary>
    ///     Class describing a LINQ parameter and its bound expression. For instance, in
    ///     products.Select(p => p.ID)
    ///     the 'products' query is the bound expression, and 'p' is the parameter.
    /// </summary>
    internal sealed class Binding
    {
        internal Binding(Expression linqExpression, DbExpression cqtExpression)
        {
            DebugCheck.NotNull(linqExpression);
            DebugCheck.NotNull(cqtExpression);

            LinqExpression = linqExpression;
            CqtExpression = cqtExpression;
        }

        internal readonly Expression LinqExpression;
        internal readonly DbExpression CqtExpression;
    }
}
