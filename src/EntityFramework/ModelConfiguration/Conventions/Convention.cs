// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;

    public class Convention : IConvention
    {
        private readonly ConventionsConfiguration _conventionsConfiguration = new ConventionsConfiguration(Enumerable.Empty<IConvention>());

        public Convention()
        {
        }

        /// <summary>
        ///     For testing
        /// </summary>
        internal Convention(ConventionsConfiguration conventionsConfiguration)
        {
            _conventionsConfiguration = conventionsConfiguration;
        }

        /// <summary>
        ///     Begins configuration of a lightweight convention that applies to all entities in
        ///     the model.
        /// </summary>
        /// <returns> A configuration object for the convention. </returns>
        public EntityConventionConfiguration Entities()
        {
            return new EntityConventionConfiguration(_conventionsConfiguration);
        }

        /// <summary>
        ///     Begins configuration of a lightweight convention that applies to all entities of
        ///     the specified type in the model. This method does not register entity types as
        ///     part of the model.
        /// </summary>
        /// <typeparam name="T"> The type of the entities that this convention will apply to. </typeparam>
        /// <returns> A configuration object for the convention. </returns>
        public EntityConventionOfTypeConfiguration<T> Entities<T>()
            where T : class
        {
            return new EntityConventionOfTypeConfiguration<T>(_conventionsConfiguration);
        }

        /// <summary>
        ///     Begins configuration of a lightweight convention that applies to all properties
        ///     in the model.
        /// </summary>
        /// <returns> A configuration object for the convention. </returns>
        public PropertyConventionConfiguration Properties()
        {
            return new PropertyConventionConfiguration(_conventionsConfiguration);
        }

        /// <summary>
        ///     Begins configuration of a lightweight convention that applies to all primitive
        ///     properties of the specified type in the model.
        /// </summary>
        /// <typeparam name="T"> The type of the properties that the convention will apply to. </typeparam>
        /// <returns> A configuration object for the convention. </returns>
        /// <remarks>
        ///     The convention will apply to both nullable and non-nullable properties of the
        ///     specified type.
        /// </remarks>
        [SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
        public PropertyConventionConfiguration Properties<T>()
        {
            if (!typeof(T).IsValidEdmScalarType())
            {
                throw Error.ModelBuilder_PropertyFilterTypeMustBePrimitive(typeof(T));
            }

            var config = new PropertyConventionConfiguration(_conventionsConfiguration);

            return config.Where(
                p =>
                    {
                        Type propertyType;
                        p.PropertyType.TryUnwrapNullableType(out propertyType);

                        return propertyType == typeof(T);
                    });
        }

        internal virtual void ApplyModelConfiguration(ModelConfiguration modelConfiguration)
        {
            _conventionsConfiguration.ApplyModelConfiguration(modelConfiguration);
        }

        internal virtual void ApplyModelConfiguration(Type type, ModelConfiguration modelConfiguration)
        {
            _conventionsConfiguration.ApplyModelConfiguration(type, modelConfiguration);
        }

        internal virtual void ApplyTypeConfiguration<TStructuralTypeConfiguration>(
            Type type, Func<TStructuralTypeConfiguration> structuralTypeConfiguration)
            where TStructuralTypeConfiguration : StructuralTypeConfiguration
        {
            _conventionsConfiguration.ApplyTypeConfiguration(type, structuralTypeConfiguration);
        }

        internal virtual void ApplyPropertyConfiguration(PropertyInfo propertyInfo, ModelConfiguration modelConfiguration)
        {
            _conventionsConfiguration.ApplyPropertyConfiguration(propertyInfo, modelConfiguration);
        }

        internal virtual void ApplyPropertyConfiguration(
            PropertyInfo propertyInfo, Func<PropertyConfiguration> propertyConfiguration)
        {
            _conventionsConfiguration.ApplyPropertyConfiguration(propertyInfo, propertyConfiguration);
        }

        internal virtual void ApplyPropertyTypeConfiguration<TStructuralTypeConfiguration>(
            PropertyInfo propertyInfo, Func<TStructuralTypeConfiguration> structuralTypeConfiguration)
            where TStructuralTypeConfiguration : StructuralTypeConfiguration
        {
            _conventionsConfiguration.ApplyPropertyTypeConfiguration(propertyInfo, structuralTypeConfiguration);
        }
    }
}
