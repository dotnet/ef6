namespace System.Data.Entity.ModelConfiguration.Conventions.Sets
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    internal static class V2ConventionSet
    {
        private static readonly IConvention[] _conventions;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static V2ConventionSet()
        {
            var conventions = new List<IConvention>(V1ConventionSet.Conventions);

            var columnOrderingConventionIndex
                = conventions.FindIndex(c => c.GetType() == typeof(ColumnOrderingConvention));

            Contract.Assert(columnOrderingConventionIndex != -1);

            conventions[columnOrderingConventionIndex] = new ColumnOrderingConventionStrict();

            _conventions = conventions.ToArray();
        }

        public static IConvention[] Conventions
        {
            get { return _conventions; }
        }
    }
}
