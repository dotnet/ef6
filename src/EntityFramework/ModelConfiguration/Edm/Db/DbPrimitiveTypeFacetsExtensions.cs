// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Edm.Db
{
    using System.Data.Entity.Edm.Db;
    using System.Diagnostics.Contracts;

    internal static class DbPrimitiveTypeFacetsExtensions
    {
        public static DbPrimitiveTypeFacets Clone(this DbPrimitiveTypeFacets toClone)
        {
            Contract.Requires(toClone != null);

            var clone = new DbPrimitiveTypeFacets();

            clone.CopyFrom(toClone);

            return clone;
        }

        public static void CopyFrom(this DbPrimitiveTypeFacets facets, DbPrimitiveTypeFacets other)
        {
            Contract.Requires(facets != null);
            Contract.Requires(other != null);

            facets.IsFixedLength = other.IsFixedLength;
            facets.IsMaxLength = other.IsMaxLength;
            facets.IsUnicode = other.IsUnicode;
            facets.MaxLength = other.MaxLength;
            facets.Precision = other.Precision;
            facets.Scale = other.Scale;
            facets.IsVariableSrid = other.IsVariableSrid;
            facets.Srid = other.Srid;
            facets.IsStrict = other.IsStrict;
        }
    }
}
