// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Configuration.Properties;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Conventions.Sets;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;

    /// <summary>
    /// A convention that doesn't override configuration.
    /// </summary>
    public class Convention : IConvention
    {
        private readonly ConventionsConfiguration _conventionsConfiguration = new ConventionsConfiguration(new ConventionSet());

        /// <summary>
        /// The derived class can use the default constructor to apply a set rule of that change the model configuration.
        /// </summary>
        public Convention()
        {
        }

        /// <summary>
        /// For testing
        /// </summary>
        internal Convention(ConventionsConfiguration conventionsConfiguration)
        {
            _conventionsConfiguration = conventionsConfiguration;
        }

        /// <summary>
        /// Begins configuration of a lightweight convention that applies to all mapped types in
        /// the model.
        /// </summary>
        /// <returns> A configuration object for the convention. </returns>
        public TypeConventionConfiguration Types()
        {
            return new TypeConventionConfiguration(_conventionsConfiguration);
        }

        /// <summary>
        /// Begins configuration of a lightweight convention that applies to all mapped types in
        /// the model that derive from or implement the specified type.
        /// </summary>
        /// <typeparam name="T"> The type of the entities that this convention will apply to. </typeparam>
        /// <returns> A configuration object for the convention. </returns>
        /// <remarks> This method does not add new types to the model.</remarks>
        public TypeConventionConfiguration<T> Types<T>()
            where T : class
        {
            return new TypeConventionConfiguration<T>(_conventionsConfiguration);
        }

        /// <summary>
        /// Begins configuration of a lightweight convention that applies to all properties
        /// in the model.
        /// </summary>
        /// <returns> A configuration object for the convention. </returns>
        public PropertyConventionConfiguration Properties()
        {
            return new PropertyConventionConfiguration(_conventionsConfiguration);
        }

        /// <summary>
        /// Begins configuration of a lightweight convention that applies to all primitive
        /// properties of the specified type in the model.
        /// </summary>
        /// <typeparam name="T"> The type of the properties that the convention will apply to. </typeparam>
        /// <returns> A configuration object for the convention. </returns>
        /// <remarks>
        /// The convention will apply to both nullable and non-nullable properties of the
        /// specified type.
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
            Type type,
            Func<TStructuralTypeConfiguration> structuralTypeConfiguration,
            ModelConfiguration modelConfiguration)
            where TStructuralTypeConfiguration : StructuralTypeConfiguration
        {
            _conventionsConfiguration.ApplyTypeConfiguration(type, structuralTypeConfiguration, modelConfiguration);
        }

        internal virtual void ApplyPropertyConfiguration(PropertyInfo propertyInfo, ModelConfiguration modelConfiguration)
        {
            _conventionsConfiguration.ApplyPropertyConfiguration(propertyInfo, modelConfiguration);
        }

        internal virtual void ApplyPropertyConfiguration(
            PropertyInfo propertyInfo,
            Func<PropertyConfiguration> propertyConfiguration,
            ModelConfiguration modelConfiguration)
        {
            _conventionsConfiguration.ApplyPropertyConfiguration(propertyInfo, propertyConfiguration, modelConfiguration);
        }

        internal virtual void ApplyPropertyTypeConfiguration<TStructuralTypeConfiguration>(
            PropertyInfo propertyInfo,
            Func<TStructuralTypeConfiguration> structuralTypeConfiguration,
            ModelConfiguration modelConfiguration)
            where TStructuralTypeConfiguration : StructuralTypeConfiguration
        {
            _conventionsConfiguration.ApplyPropertyTypeConfiguration(propertyInfo, structuralTypeConfiguration, modelConfiguration);
        }
    }
}
