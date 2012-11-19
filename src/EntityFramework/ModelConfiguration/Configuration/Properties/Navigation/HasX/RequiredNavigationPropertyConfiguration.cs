// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     Configures an required relationship from an entity type.
    /// </summary>
    /// <typeparam name="TEntityType"> The entity type that the relationship originates from. </typeparam>
    /// <typeparam name="TTargetEntityType"> The entity type that the relationship targets. </typeparam>
    public class RequiredNavigationPropertyConfiguration<TEntityType, TTargetEntityType>
        where TEntityType : class
        where TTargetEntityType : class
    {
        private readonly NavigationPropertyConfiguration _navigationPropertyConfiguration;

        internal RequiredNavigationPropertyConfiguration(
            NavigationPropertyConfiguration navigationPropertyConfiguration)
        {
            DebugCheck.NotNull(navigationPropertyConfiguration);

            navigationPropertyConfiguration.Reset();
            _navigationPropertyConfiguration = navigationPropertyConfiguration;
            _navigationPropertyConfiguration.RelationshipMultiplicity = RelationshipMultiplicity.One;
        }

        /// <summary>
        ///     Configures the relationship to be required:many with a navigation property on the other side of the relationship.
        /// </summary>
        /// <param name="navigationPropertyExpression"> An lambda expression representing the navigation property on the other end of the relationship. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public DependentNavigationPropertyConfiguration<TEntityType> WithMany(
            Expression<Func<TTargetEntityType, ICollection<TEntityType>>> navigationPropertyExpression)
        {
            Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");

            _navigationPropertyConfiguration.InverseNavigationProperty
                = navigationPropertyExpression.GetSimplePropertyAccess().Single();

            return WithMany();
        }

        /// <summary>
        ///     Configures the relationship to be required:many without a navigation property on the other side of the relationship.
        /// </summary>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        public DependentNavigationPropertyConfiguration<TEntityType> WithMany()
        {
            _navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.Many;

            return new DependentNavigationPropertyConfiguration<TEntityType>(_navigationPropertyConfiguration);
        }

        /// <summary>
        ///     Configures the relationship to be required:optional with a navigation property on the other side of the relationship.
        /// </summary>
        /// <param name="navigationPropertyExpression"> An lambda expression representing the navigation property on the other end of the relationship. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public ForeignKeyNavigationPropertyConfiguration WithOptional(
            Expression<Func<TTargetEntityType, TEntityType>> navigationPropertyExpression)
        {
            Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");

            _navigationPropertyConfiguration.InverseNavigationProperty
                = navigationPropertyExpression.GetSimplePropertyAccess().Single();

            return WithOptional();
        }

        /// <summary>
        ///     Configures the relationship to be required:optional without a navigation property on the other side of the relationship.
        /// </summary>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        public ForeignKeyNavigationPropertyConfiguration WithOptional()
        {
            _navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.ZeroOrOne;

            return new ForeignKeyNavigationPropertyConfiguration(_navigationPropertyConfiguration);
        }

        /// <summary>
        ///     Configures the relationship to be required:required with a navigation property on the other side of the relationship.
        ///     The entity type being configured will be the dependent and contain a foreign key to the principal.
        ///     The entity type that the relationship targets will be the principal in the relationship.
        /// </summary>
        /// <param name="navigationPropertyExpression"> An lambda expression representing the navigation property on the other end of the relationship. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public ForeignKeyNavigationPropertyConfiguration WithRequiredDependent(
            Expression<Func<TTargetEntityType, TEntityType>> navigationPropertyExpression)
        {
            Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");

            _navigationPropertyConfiguration.InverseNavigationProperty
                = navigationPropertyExpression.GetSimplePropertyAccess().Single();

            return WithRequiredDependent();
        }

        /// <summary>
        ///     Configures the relationship to be required:required without a navigation property on the other side of the relationship.
        ///     The entity type being configured will be the dependent and contain a foreign key to the principal.
        ///     The entity type that the relationship targets will be the principal in the relationship.
        /// </summary>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        public ForeignKeyNavigationPropertyConfiguration WithRequiredDependent()
        {
            _navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.One;

            _navigationPropertyConfiguration.IsNavigationPropertyDeclaringTypePrincipal = false;

            return new ForeignKeyNavigationPropertyConfiguration(_navigationPropertyConfiguration);
        }

        /// <summary>
        ///     Configures the relationship to be required:required with a navigation property on the other side of the relationship.
        ///     The entity type being configured will be the principal in the relationship.
        ///     The entity type that the relationship targets will be the dependent and contain a foreign key to the principal.
        /// </summary>
        /// <param name="navigationPropertyExpression"> An lambda expression representing the navigation property on the other end of the relationship. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public ForeignKeyNavigationPropertyConfiguration WithRequiredPrincipal(
            Expression<Func<TTargetEntityType, TEntityType>> navigationPropertyExpression)
        {
            Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");

            _navigationPropertyConfiguration.InverseNavigationProperty
                = navigationPropertyExpression.GetSimplePropertyAccess().Single();

            return WithRequiredPrincipal();
        }

        /// <summary>
        ///     Configures the relationship to be required:required without a navigation property on the other side of the relationship.
        ///     The entity type being configured will be the principal in the relationship.
        ///     The entity type that the relationship targets will be the dependent and contain a foreign key to the principal.
        /// </summary>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        public ForeignKeyNavigationPropertyConfiguration WithRequiredPrincipal()
        {
            _navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.One;

            _navigationPropertyConfiguration.IsNavigationPropertyDeclaringTypePrincipal = true;

            return new ForeignKeyNavigationPropertyConfiguration(_navigationPropertyConfiguration);
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
