namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.Contracts;

    internal static class EdmEntitySetExtensions
    {
        public static object GetConfiguration(this EdmEntitySet entitySet)
        {
            Contract.Requires(entitySet != null);

            return entitySet.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this EdmEntitySet entitySet, object configuration)
        {
            Contract.Requires(entitySet != null);

            entitySet.Annotations.SetConfiguration(configuration);
        }
    }
}
