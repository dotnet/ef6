namespace System.Data.Entity.ModelConfiguration.Conventions.Sets
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    internal static class V2ConventionSet
    {
        public static readonly IConvention[] Conventions;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static V2ConventionSet()
        {
            var conventions = new List<IConvention>(V1ConventionSet.Conventions);

            var columnOrderingConventionIndex
                = conventions.FindIndex(c => c.GetType() == typeof(ColumnOrderingConvention));

            Contract.Assert(columnOrderingConventionIndex != -1);

            conventions[columnOrderingConventionIndex] = new ColumnOrderingConventionStrict();

            Conventions = conventions.ToArray();
        }
    }
}
