// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.Sets
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    internal static class V2ConventionSet
    {
        private static readonly ConventionSet _conventions;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static V2ConventionSet()
        {
            var dbConventions = new List<IConvention>(V1ConventionSet.Conventions.StoreModelConventions);

            var columnOrderingConventionIndex
                = dbConventions.FindIndex(c => c.GetType() == typeof(ColumnOrderingConvention));

            Debug.Assert(columnOrderingConventionIndex != -1);

            dbConventions[columnOrderingConventionIndex] = new ColumnOrderingConventionStrict();

            _conventions = new ConventionSet(
                V1ConventionSet.Conventions.ConfigurationConventions,
                V1ConventionSet.Conventions.ConceptualModelConventions,
                V1ConventionSet.Conventions.ConceptualToStoreMappingConventions,
                dbConventions);
        }

        public static ConventionSet Conventions
        {
            get { return _conventions; }
        }
    }
}
