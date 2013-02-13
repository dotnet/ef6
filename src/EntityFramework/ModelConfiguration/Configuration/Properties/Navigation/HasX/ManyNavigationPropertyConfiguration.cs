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
    ///     Configures a many relationship from an entity type.
    /// </summary>
    /// <typeparam name="TEntityType"> The entity type that the relationship originates from. </typeparam>
    /// <typeparam name="TTargetEntityType"> The entity type that the relationship targets. </typeparam>
    public class ManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType>
        where TEntityType : class
        where TTargetEntityType : class
    {
        private readonly NavigationPropertyConfiguration _navigationPropertyConfiguration;

        internal ManyNavigationPropertyConfiguration(NavigationPropertyConfiguration navigationPropertyConfiguration)
        {
            DebugCheck.NotNull(navigationPropertyConfiguration);

            navigationPropertyConfiguration.Reset();
            _navigationPropertyConfiguration = navigationPropertyConfiguration;
            _navigationPropertyConfiguration.RelationshipMultiplicity = RelationshipMultiplicity.Many;
        }

        /// <summary>
        ///     Configures the relationship to be many:many with a navigation property on the other side of the relationship.
        /// </summary>
        /// <param name="navigationPropertyExpression"> An lambda expression representing the navigation property on the other end of the relationship. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> WithMany(
            Expression<Func<TTargetEntityType, ICollection<TEntityType>>> navigationPropertyExpression)
        {
            Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");

            _navigationPropertyConfiguration.InverseNavigationProperty
                = navigationPropertyExpression.GetSimplePropertyAccess().Single();

            return WithMany();
        }

        /// <summary>
        ///     Configures the relationship to be many:many without a navigation property on the other side of the relationship.
        /// </summary>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        public ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType> WithMany()
        {
            _navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.Many;

            return new ManyToManyNavigationPropertyConfiguration<TEntityType, TTargetEntityType>(_navigationPropertyConfiguration);
        }

        /// <summary>
        ///     Configures the relationship to be many:required with a navigation property on the other side of the relationship.
        /// </summary>
        /// <param name="navigationPropertyExpression"> An lambda expression representing the navigation property on the other end of the relationship. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public DependentNavigationPropertyConfiguration<TTargetEntityType> WithRequired(
            Expression<Func<TTargetEntityType, TEntityType>> navigationPropertyExpression)
        {
            Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");

            _navigationPropertyConfiguration.InverseNavigationProperty
                = navigationPropertyExpression.GetSimplePropertyAccess().Single();

            return WithRequired();
        }

        /// <summary>
        ///     Configures the relationship to be many:required without a navigation property on the other side of the relationship.
        /// </summary>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        public DependentNavigationPropertyConfiguration<TTargetEntityType> WithRequired()
        {
            _navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.One;

            return new DependentNavigationPropertyConfiguration<TTargetEntityType>(_navigationPropertyConfiguration);
        }

        /// <summary>
        ///     Configures the relationship to be many:optional with a navigation property on the other side of the relationship.
        /// </summary>
        /// <param name="navigationPropertyExpression"> An lambda expression representing the navigation property on the other end of the relationship. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public DependentNavigationPropertyConfiguration<TTargetEntityType> WithOptional(
            Expression<Func<TTargetEntityType, TEntityType>> navigationPropertyExpression)
        {
            Check.NotNull(navigationPropertyExpression, "navigationPropertyExpression");

            _navigationPropertyConfiguration.InverseNavigationProperty
                = navigationPropertyExpression.GetSimplePropertyAccess().Single();

            return WithOptional();
        }

        /// <summary>
        ///     Configures the relationship to be many:optional without a navigation property on the other side of the relationship.
        /// </summary>
        /// <returns> A configuration object that can be used to further configure the relationship. </returns>
        public DependentNavigationPropertyConfiguration<TTargetEntityType> WithOptional()
        {
            _navigationPropertyConfiguration.InverseEndKind = RelationshipMultiplicity.ZeroOrOne;

            return new DependentNavigationPropertyConfiguration<TTargetEntityType>(_navigationPropertyConfiguration);
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
