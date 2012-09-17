// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Types
{
    using System.Data.Entity.Edm;

    public class ComplexTypeConfiguration : StructuralTypeConfiguration
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

        internal virtual void Configure(EdmComplexType complexType)
        {
            Configure(complexType.Name, complexType.DeclaredProperties, complexType.Annotations);
        }
    }
}
