// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     Allows derived configuration classes for entities and complex types to be registered with a <see cref = "DbModelBuilder" />.
    /// </summary>
    /// <remarks>
    ///     Derived configuration classes are created by deriving from <see cref = "EntityTypeConfiguration" />
    ///     or <see cref = "ComplexTypeConfiguration" /> and using a type to be included in the model as the generic
    ///     parameter.
    /// 
    ///     Configuration can be performed without creating derived configuration classes via the Entity and ComplexType
    ///     methods on <see cref = "DbModelBuilder" />.
    /// </remarks>
    public class ConfigurationRegistrar
    {
        private readonly ModelConfiguration _modelConfiguration;

        internal ConfigurationRegistrar(ModelConfiguration modelConfiguration)
        {
            Contract.Requires(modelConfiguration != null);

            _modelConfiguration = modelConfiguration;
        }

        /// <summary>
        ///     Adds an <see cref = "EntityTypeConfiguration" /> to the <see cref = "DbModelBuilder" />.
        ///     Only one <see cref = "EntityTypeConfiguration" /> can be added for each type in a model.
        /// </summary>
        /// <typeparam name = "TEntityType">The entity type being configured.</typeparam>
        /// <param name = "entityTypeConfiguration">The entity type configuration to be added.</param>
        /// <returns>The same ConfigurationRegistrar instance so that multiple calls can be chained.</returns>
        public virtual ConfigurationRegistrar Add<TEntityType>(
            EntityTypeConfiguration<TEntityType> entityTypeConfiguration)
            where TEntityType : class
        {
            Contract.Requires(entityTypeConfiguration != null);
            Contract.Assert(entityTypeConfiguration.Configuration != null);

            _modelConfiguration.Add((EntityTypeConfiguration)entityTypeConfiguration.Configuration);

            return this;
        }

        /// <summary>
        ///     Adds an <see cref = "ComplexTypeConfiguration" /> to the <see cref = "DbModelBuilder" />.
        ///     Only one <see cref = "ComplexTypeConfiguration" /> can be added for each type in a model.
        /// </summary>
        /// <typeparam name = "TComplexType">The complex type being configured.</typeparam>
        /// <param name = "complexTypeConfiguration">The complex type configuration to be added</param>
        /// <returns>The same ConfigurationRegistrar instance so that multiple calls can be chained.</returns>
        public virtual ConfigurationRegistrar Add<TComplexType>(
            ComplexTypeConfiguration<TComplexType> complexTypeConfiguration)
            where TComplexType : class
        {
            Contract.Requires(complexTypeConfiguration != null);
            Contract.Assert(complexTypeConfiguration.Configuration != null);

            _modelConfiguration.Add((ComplexTypeConfiguration)complexTypeConfiguration.Configuration);

            return this;
        }

        internal virtual IEnumerable<Type> GetConfiguredTypes()
        {
            return _modelConfiguration.ConfiguredTypes.ToList();
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
