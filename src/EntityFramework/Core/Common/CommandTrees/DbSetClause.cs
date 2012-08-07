// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Common.Utils;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Specifies a clause in a modification operation setting the value of a property.
    /// </summary>
    public sealed class DbSetClause : DbModificationClause
    {
        private readonly DbExpression _prop;
        private readonly DbExpression _val;

        internal DbSetClause(DbExpression targetProperty, DbExpression sourceValue)
        {
            Contract.Requires(targetProperty != null);
            Contract.Requires(sourceValue != null);
            _prop = targetProperty;
            _val = sourceValue;
        }

        /// <summary>
        ///     Gets an <see cref="DbExpression" /> that specifies the property that should be updated.
        /// </summary>
        /// <remarks>
        ///     Constrained to be a <see cref="DbPropertyExpression" />.
        /// </remarks>
        public DbExpression Property
        {
            get { return _prop; }
        }

        /// <summary>
        ///     Gets an <see cref="DbExpression" /> that specifies the new value with which to update the property.
        /// </summary>
        /// <remarks>
        ///     Constrained to be a <see cref="DbConstantExpression" /> or <see cref="DbNullExpression" />
        /// </remarks>
        public DbExpression Value
        {
            get { return _val; }
        }

        internal override void DumpStructure(ExpressionDumper dumper)
        {
            dumper.Begin("DbSetClause");
            if (null != Property)
            {
                dumper.Dump(Property, "Property");
            }
            if (null != Value)
            {
                dumper.Dump(Value, "Value");
            }
            dumper.End("DbSetClause");
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "DbSetClause")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Common.Utils.TreeNode.#ctor(System.String,System.Data.Entity.Core.Common.Utils.TreeNode[])"
            )]
        internal override TreeNode Print(DbExpressionVisitor<TreeNode> visitor)
        {
            var node = new TreeNode("DbSetClause");
            if (null != Property)
            {
                node.Children.Add(new TreeNode("Property", Property.Accept(visitor)));
            }
            if (null != Value)
            {
                node.Children.Add(new TreeNode("Value", Value.Accept(visitor)));
            }
            return node;
        }
    }
}
