// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration
{
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    /// Allows configuration to be performed for an complex type in a model.
    /// A ComplexTypeConfiguration can be obtained via the ComplexType method on
    /// <see cref="DbModelBuilder" /> or a custom type derived from ComplexTypeConfiguration
    /// can be registered via the Configurations property on <see cref="DbModelBuilder" />.
    /// </summary>
    /// <typeparam name="TComplexType"> The complex type to be configured. </typeparam>
    public class ComplexTypeConfiguration<TComplexType> : StructuralTypeConfiguration<TComplexType>
        where TComplexType : class
    {
        private readonly ComplexTypeConfiguration _complexTypeConfiguration;

        /// <summary>
        /// Initializes a new instance of ComplexTypeConfiguration
        /// </summary>
        public ComplexTypeConfiguration()
            : this(new ComplexTypeConfiguration(typeof(TComplexType)))
        {
        }

        /// <summary>
        /// Excludes a property from the model so that it will not be mapped to the database.
        /// </summary>
        /// <typeparam name="TProperty"> The type of the property to be ignored. </typeparam>
        /// <param name="propertyExpression"> A lambda expression representing the property to be configured. C#: t => t.MyProperty VB.Net: Function(t) t.MyProperty </param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public ComplexTypeConfiguration<TComplexType> Ignore<TProperty>(Expression<Func<TComplexType, TProperty>> propertyExpression)
        {
            Check.NotNull(propertyExpression, "propertyExpression");

            Configuration.Ignore(propertyExpression.GetSimplePropertyAccess().Single());

            return this;
        }

        internal ComplexTypeConfiguration(ComplexTypeConfiguration configuration)
        {
            _complexTypeConfiguration = configuration;
        }

        internal override StructuralTypeConfiguration Configuration
        {
            get { return _complexTypeConfiguration; }
        }

        internal override TPrimitivePropertyConfiguration Property<TPrimitivePropertyConfiguration>(
            LambdaExpression lambdaExpression)
        {
            return Configuration.Property(
                lambdaExpression.GetSimplePropertyAccess(),
                () =>
                new TPrimitivePropertyConfiguration
                    {
                        OverridableConfigurationParts = OverridableConfigurationParts.OverridableInSSpace
                    });
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <inheritdoc />
        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}
