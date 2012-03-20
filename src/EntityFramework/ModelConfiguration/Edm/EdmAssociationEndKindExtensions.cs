namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Edm;

    internal static class EdmAssociationEndKindExtensions
    {
        public static bool IsMany(this EdmAssociationEndKind associationEndKind)
        {
            return associationEndKind == EdmAssociationEndKind.Many;
        }

        public static bool IsOptional(this EdmAssociationEndKind associationEndKind)
        {
            return associationEndKind == EdmAssociationEndKind.Optional;
        }

        public static bool IsRequired(this EdmAssociationEndKind associationEndKind)
        {
            return associationEndKind == EdmAssociationEndKind.Required;
        }
    }
}