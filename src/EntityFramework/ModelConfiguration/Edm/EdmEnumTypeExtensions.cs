namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.Contracts;

    internal static class EdmEnumTypeExtensions
    {
        public static Type GetClrType(this EdmEnumType enumType)
        {
            Contract.Requires(enumType != null);

            return enumType.Annotations.GetClrType();
        }

        public static void SetClrType(this EdmEnumType enumType, Type type)
        {
            Contract.Requires(enumType != null);
            Contract.Requires(type != null);

            enumType.Annotations.SetClrType(type);
        }

        public static EdmEnumTypeMember AddMember(this EdmEnumType enumType, string name, long value)
        {
            Contract.Requires(enumType != null);
            Contract.Requires(!string.IsNullOrWhiteSpace(name));

            var enumTypeMember = new EdmEnumTypeMember { Name = name, Value = value };

            enumType.Members.Add(enumTypeMember);

            return enumTypeMember;
        }
    }
}