// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    internal static class ComplexTypeExtensions
    {
        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode",
            Justification = "Used by test code.")]
        public static EdmProperty AddComplexProperty(
            this ComplexType complexType, string name, ComplexType targetComplexType)
        {
            DebugCheck.NotNull(complexType);
            DebugCheck.NotNull(complexType.Properties);
            DebugCheck.NotEmpty(name);
            DebugCheck.NotNull(targetComplexType);

            var property = EdmProperty.CreateComplex(name, targetComplexType);

            complexType.AddMember(property);

            return property;
        }

        public static object GetConfiguration(this ComplexType complexType)
        {
            DebugCheck.NotNull(complexType);

            return complexType.Annotations.GetConfiguration();
        }

        public static Type GetClrType(this ComplexType complexType)
        {
            DebugCheck.NotNull(complexType);

            return complexType.Annotations.GetClrType();
        }

        internal static IEnumerable<ComplexType> ToHierarchy(this ComplexType edmType)
        {
            return EdmType.SafeTraverseHierarchy(edmType);
        }
    }
}
