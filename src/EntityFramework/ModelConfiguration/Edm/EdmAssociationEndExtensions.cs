namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Edm;

    internal static class EdmAssociationEndExtensions
    {
        public static bool IsMany(this EdmAssociationEnd associationEnd)
        {
            return associationEnd.EndKind.IsMany();
        }

        public static bool IsOptional(this EdmAssociationEnd associationEnd)
        {
            return associationEnd.EndKind.IsOptional();
        }

        public static bool IsRequired(this EdmAssociationEnd associationEnd)
        {
            return associationEnd.EndKind.IsRequired();
        }
    }
}
