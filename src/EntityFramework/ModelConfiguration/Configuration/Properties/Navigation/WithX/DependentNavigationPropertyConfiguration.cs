// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     Configures a relationship that can support foreign key properties that are exposed in the object model.
    ///     This configuration functionality is available via the Code First Fluent API, see <see cref = "DbModelBuilder" />.
    /// </summary>
    /// <typeparam name = "TDependentEntityType">The dependent entity type.</typeparam>
    public class DependentNavigationPropertyConfiguration<TDependentEntityType> :
        ForeignKeyNavigationPropertyConfiguration
        where TDependentEntityType : class
    {
        internal DependentNavigationPropertyConfiguration(
            NavigationPropertyConfiguration navigationPropertyConfiguration)
            : base(navigationPropertyConfiguration)
        {
        }

        /// <summary>
        ///     Configures the relationship to use foreign key property(s) that are exposed in the object model.
        ///     If the foreign key property(s) are not exposed in the object model then use the Map method.
        /// </summary>
        /// <typeparam name = "TKey">The type of the key.</typeparam>
        /// <param name = "foreignKeyExpression">
        ///     A lambda expression representing the property to be used as the foreign key. 
        ///     If the foreign key is made up of multiple properties then specify an anonymous type including the properties. 
        ///     When using multiple foreign key properties, the properties must be specified in the same order that the
        ///     the primary key properties were configured for the principal entity type.
        /// </param>
        /// <returns>A configuration object that can be used to further configure the relationship.</returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public CascadableNavigationPropertyConfiguration HasForeignKey<TKey>(
            Expression<Func<TDependentEntityType, TKey>> foreignKeyExpression)
        {
            Contract.Requires(foreignKeyExpression != null);

            NavigationPropertyConfiguration.Constraint
                = new ForeignKeyConstraintConfiguration(
                    foreignKeyExpression.GetSimplePropertyAccessList()
                        .Select(p => p.Single()));

            return this;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
