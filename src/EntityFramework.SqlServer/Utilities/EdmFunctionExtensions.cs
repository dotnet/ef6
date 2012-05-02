namespace System.Data.Entity.SqlServer.Utilities
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Linq;

    internal static class EdmFunctionExtensions
    {
        internal static bool IsCSpace(this EdmFunction function)
        {
            Contract.Requires(function != null);

            var property = function.MetadataProperties.FirstOrDefault(p => p.Name == "DataSpace");
            return property != null && (DataSpace)property.Value == DataSpace.CSpace;
        }

        internal static bool IsCanonicalFunction(this EdmFunction function)
        {
            Contract.Requires(function != null);

            return (IsCSpace(function) && function.NamespaceName == "Edm");
        }
    }
}