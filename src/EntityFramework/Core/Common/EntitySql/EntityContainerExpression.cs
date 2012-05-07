namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;

    /// <summary>
    /// Represents an eSQL expression classified as <see cref="ExpressionResolutionClass.EntityContainer"/>.
    /// </summary>
    internal sealed class EntityContainerExpression : ExpressionResolution
    {
        internal EntityContainerExpression(EntityContainer entityContainer)
            : base(ExpressionResolutionClass.EntityContainer)
        {
            EntityContainer = entityContainer;
        }

        internal override string ExpressionClassName
        {
            get { return EntityContainerClassName; }
        }

        internal static string EntityContainerClassName
        {
            get { return Strings.LocalizedEntityContainerExpression; }
        }

        internal readonly EntityContainer EntityContainer;
    }
}