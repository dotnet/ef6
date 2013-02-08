// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    internal static class EnumTypeExtensions
    {
        public static Type GetClrType(this EnumType enumType)
        {
            DebugCheck.NotNull(enumType);

            return enumType.Annotations.GetClrType();
        }

        public static void SetClrType(this EnumType enumType, Type type)
        {
            DebugCheck.NotNull(enumType);
            DebugCheck.NotNull(type);

            enumType.Annotations.SetClrType(type);
        }
    }
}
