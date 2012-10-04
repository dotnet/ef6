// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.Contracts;

    internal static class EdmEnumTypeExtensions
    {
        public static Type GetClrType(this EnumType enumType)
        {
            Contract.Requires(enumType != null);

            return enumType.Annotations.GetClrType();
        }

        public static void SetClrType(this EnumType enumType, Type type)
        {
            Contract.Requires(enumType != null);
            Contract.Requires(type != null);

            enumType.Annotations.SetClrType(type);
        }
    }
}
