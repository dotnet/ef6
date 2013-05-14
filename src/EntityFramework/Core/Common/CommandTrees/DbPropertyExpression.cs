// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Provides methods and properties for retrieving an instance property. This class cannot be inherited.</summary>
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
            DebugCheck.NotNull(property);
            DebugCheck.NotNull(instance);
            Debug.Assert(
                Helper.IsEdmProperty(property) ||
                Helper.IsRelationshipEndMember(property) ||
                Helper.IsNavigationProperty(property), "DbExpression property must be a property, navigation property, or relationship end");

            _property = property;
            _instance = instance;
        }

        /// <summary>Gets the property metadata for the property to retrieve.</summary>
        /// <returns>The property metadata for the property to retrieve.</returns>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
        public virtual EdmMember Property
        {
            get { return _property; }
        }

        /// <summary>
        ///     Gets a <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the instance from which the property should be retrieved.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpression" /> that defines the instance from which the property should be retrieved.
        /// </returns>
        /// <exception cref="T:System.ArgumentNullException">The expression is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        ///     The expression is not associated with the command tree of the
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression" />
        ///     , or its result type is not equal or promotable to the type that defines the property.
        /// </exception>
        public virtual DbExpression Instance
        {
            get { return _instance; }
        }

        /// <summary>Implements the visitor pattern for expressions that do not produce a result value.</summary>
        /// <param name="visitor">
        ///     An instance of <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override void Accept(DbExpressionVisitor visitor)
        {
            Check.NotNull(visitor, "visitor");

            visitor.Visit(this);
        }

        /// <summary>Implements the visitor pattern for expressions that produce a result value of a specific type.</summary>
        /// <returns>
        ///     A result value of a specific type produced by
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" />
        ///     .
        /// </returns>
        /// <param name="visitor">
        ///     An instance of a typed <see cref="T:System.Data.Entity.Core.Common.CommandTrees.DbExpressionVisitor" /> that produces a result value of a specific type.
        /// </param>
        /// <typeparam name="TResultType">The type of the result produced by  visitor .</typeparam>
        /// <exception cref="T:System.ArgumentNullException"> visitor  is null.</exception>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor)
        {
            Check.NotNull(visitor, "visitor");

            return visitor.Visit(this);
        }

        /// <summary>Creates a new key/value pair based on this property expression.</summary>
        /// <returns>
        ///     A new key/value pair with the key and value derived from the
        ///     <see
        ///         cref="T:System.Data.Entity.Core.Common.CommandTrees.DbPropertyExpression" />
        ///     .
        /// </returns>
        public KeyValuePair<string, DbExpression> ToKeyValuePair()
        {
            return new KeyValuePair<string, DbExpression>(Property.Name, this);
        }

        public static implicit operator KeyValuePair<string, DbExpression>(DbPropertyExpression value)
        {
            Check.NotNull(value, "value");

            return value.ToKeyValuePair();
        }
    }
}
