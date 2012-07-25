// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Represents the retrieval of a static or instance property.
    /// </summary>
    public class DbPropertyExpression : DbExpression
    {
        private readonly EdmMember _property;
        private readonly DbExpression _instance;

        internal DbPropertyExpression()
        {
        }

        internal DbPropertyExpression(TypeUsage resultType, EdmMember property, DbExpression instance)
            : base(DbExpressionKind.Property, resultType)
        {
            Debug.Assert(property != null, "DbPropertyExpression property cannot be null");
            Debug.Assert(instance != null, "DbPropertyExpression instance cannot be null");
            Debug.Assert(
                Helper.IsEdmProperty(property) ||
                Helper.IsRelationshipEndMember(property) ||
                Helper.IsNavigationProperty(property), "DbExpression property must be a property, navigation property, or relationship end");

            _property = property;
            _instance = instance;
        }

        /// <summary>
        /// Gets the property metadata for the property to retrieve.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
        public virtual EdmMember Property
        {
            get { return _property; }
        }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> that defines the instance from which the property should be retrieved.
        /// </summary>
        public virtual DbExpression Instance
        {
            get { return _instance; }
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            if (visitor != null)
            {
                visitor.Visit(this);
            }
            else
            {
                throw new ArgumentNullException("visitor");
            }
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            if (visitor != null)
            {
                return visitor.Visit(this);
            }
            else
            {
                throw new ArgumentNullException("visitor");
            }
        }

        /// <summary>
        /// Creates a new KeyValuePair&lt;string, DbExpression&gt; based on this property expression.
        /// The string key will be the name of the referenced property, while the DbExpression value will be the property expression itself.
        /// </summary>
        /// <returns>A new KeyValuePair&lt;string, DbExpression&gt; with key and value derived from the DbPropertyExpression</returns>
        public KeyValuePair<string, DbExpression> ToKeyValuePair()
        {
            return new KeyValuePair<string, DbExpression>(Property.Name, this);
        }

        public static implicit operator KeyValuePair<string, DbExpression>(DbPropertyExpression value)
        {
            Contract.Requires(value != null);
            return value.ToKeyValuePair();
        }
    }
}
