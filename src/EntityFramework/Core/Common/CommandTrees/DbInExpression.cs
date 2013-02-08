// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Linq;

    /// <summary>
    ///     Represents a boolean expression that tests whether a specified item matches any element in a list.
    /// </summary>
    public class DbInExpression : DbExpression
    {
        private readonly DbExpression _item;
        private readonly DbExpressionList _list;

        internal DbInExpression(TypeUsage booleanResultType, DbExpression item, DbExpressionList list)
            : base(DbExpressionKind.In, booleanResultType)
        {
            DebugCheck.NotNull(item);
            DebugCheck.NotNull(list);
            Debug.Assert(TypeSemantics.IsBooleanType(booleanResultType), "DbInExpression must have a Boolean result type");
            Debug.Assert(list.Count > 0, "DbInExpression list must not be empy");
            Debug.Assert(list.All(e => TypeSemantics.IsEqual(e.ResultType, item.ResultType)), 
                "DbInExpression requires the same result type for the input expressions");

            _item = item;
            _list = list;
        }

        /// <summary>
        /// Gets a DbExpression that specifies the item to be matched.
        /// </summary>
        public DbExpression Item
        {
            get { return _item; }
        }

        /// <summary>
        /// Gets the list of DbExpression to test for a match.
        /// </summary>
        public IList<DbExpression> List
        {
            get { return _list; }
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor"> An instance of DbExpressionVisitor. </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        /// <summary>
        ///     The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor"> An instance of a typed DbExpressionVisitor that produces a result value of type TResultType. </param>
        /// <typeparam name="TResultType"> The type of the result produced by <paramref name="visitor" /> </typeparam>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="visitor" />
        ///     is null</exception>
        /// <returns> An instance of <typeparamref name="TResultType" /> . </returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }
    }
}
