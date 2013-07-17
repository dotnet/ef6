// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     Allows configuration to be performed for a complex type in a model.
    /// </summary>
    internal class ComplexTypeConfiguration : StructuralTypeConfiguration
    {
        internal ComplexTypeConfiguration(Type structuralType)
            : base(structuralType)
        {
        }

        private ComplexTypeConfiguration(ComplexTypeConfiguration source)
            : base(source)
        {
        }

        internal virtual ComplexTypeConfiguration Clone()
        {
            return new ComplexTypeConfiguration(this);
        }

        internal virtual void Configure(ComplexType complexType)
        {
            Configure(complexType.Name, complexType.Properties, complexType.Annotations);
        }
    }
}
