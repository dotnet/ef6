namespace System.Data.Entity.ModelConfiguration
{
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Linq.Expressions;

    /// <summary>
    ///     Allows configuration to be performed for an complex type in a model.
    /// 
    ///     A ComplexTypeConfiguration can be obtained via the ComplexType method on
    ///     <see cref = "DbModelBuilder" /> or a custom type derived from ComplexTypeConfiguration
    ///     can be registered via the Configurations property on <see cref = "DbModelBuilder" />.
    /// </summary>
    /// <typeparam name = "TComplexType">The complex type to be configured.</typeparam>
    public class ComplexTypeConfiguration<TComplexType> : StructuralTypeConfiguration<TComplexType>
        where TComplexType : class
    {
        private readonly ComplexTypeConfiguration _complexTypeConfiguration;

        /// <summary>
        ///     Initializes a new instance of ComplexTypeConfiguration
        /// </summary>
        public ComplexTypeConfiguration()
            : this(new ComplexTypeConfiguration(typeof(TComplexType)))
        {
        }

        internal ComplexTypeConfiguration(ComplexTypeConfiguration configuration)
        {
            _complexTypeConfiguration = configuration;
        }

        internal override StructuralTypeConfiguration Configuration
        {
            get { return _complexTypeConfiguration; }
        }

        internal override TPrimitivePropertyConfiguration Property<TPrimitivePropertyConfiguration>(LambdaExpression lambdaExpression)
        {
            return Configuration.Property(
                lambdaExpression.GetSimplePropertyAccess(),
                () =>
                new TPrimitivePropertyConfiguration { OverridableConfigurationParts = OverridableConfigurationParts.OverridableInSSpace });
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

        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }
    }
}