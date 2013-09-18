// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration
{
    using System.Data.Entity.Utilities;

    internal class ConfigurationTypeFilter
    {
        // <summary>
        // Check if specified type is a EntityTypeConfiguration instance.
        // </summary>
        // <param name="type">The type to check.</param>
        // <returns>True if type is a EntityTypeConfiguration, else false.</returns>
        public virtual bool IsEntityTypeConfiguration(Type type)
        {
            DebugCheck.NotNull(type);

            return IsStructuralTypeConfiguration(type, typeof(EntityTypeConfiguration<>));
        }

        // <summary>
        // Check if specified type is a ComplexTypeConfiguration instance.
        // </summary>
        // <param name="type">The type to check.</param>
        // <returns>True if type is a ComplexTypeConfiguration, else false.</returns>
        public virtual bool IsComplexTypeConfiguration(Type type)
        {
            DebugCheck.NotNull(type);

            return IsStructuralTypeConfiguration(type, typeof(ComplexTypeConfiguration<>));
        }

        private static bool IsStructuralTypeConfiguration(Type type, Type structuralTypeConfiguration)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(structuralTypeConfiguration);

            return !type.IsAbstract() && type.TryGetElementType(structuralTypeConfiguration) != null;
        }
    }
}
