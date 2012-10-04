// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    internal static class EdmComplexTypeExtensions
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmProperty AddComplexProperty(
            this ComplexType complexType, string name, ComplexType targetComplexType)
        {
            Contract.Requires(complexType != null);
            Contract.Requires(complexType.Properties != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));
            Contract.Requires(targetComplexType != null);

            var property = EdmProperty.Complex(name, targetComplexType);

            complexType.AddMember(property);

            return property;
        }

        public static object GetConfiguration(this ComplexType complexType)
        {
            Contract.Requires(complexType != null);

            return complexType.Annotations.GetConfiguration();
        }

        public static Type GetClrType(this ComplexType complexType)
        {
            Contract.Requires(complexType != null);

            return complexType.Annotations.GetClrType();
        }
    }
}
