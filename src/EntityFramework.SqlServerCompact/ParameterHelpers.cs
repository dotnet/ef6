// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.SqlServerCompact.Resources;
    using System.Data.Entity.SqlServerCompact.Utilities;
    using System.Diagnostics;

    internal static class ParameterHelpers
    {
        internal static void PopulateParameterFromTypeUsage(DbParameter parameter, TypeUsage type)
        {
            Check.NotNull(parameter, "parameter");
            Check.NotNull(type, "type");

            // parameter.Direction - take the default. we don't support output 
            // parameters.
            parameter.Direction = ParameterDirection.Input;

            // parameter.IsNullable - from the NullableConstraintAttribute value
            parameter.IsNullable = TypeSemantics.IsNullable(type);

            // parameter.ParameterName - set by the caller;
            // parameter.SourceColumn - not applicable until we have a data adapter;
            // parameter.SourceColumnNullMapping - not applicable until we have a data adapter;
            // parameter.SourceVersion - not applicable until we have a data adapter;
            // parameter.Value - left unset;
            // parameter.DbType - determined by the TypeMapping;
            // parameter.Precision - from the TypeMapping;
            // parameter.Scale - from the TypeMapping;
            // parameter.Size - from the TypeMapping;

            Debug.Assert(null != type, "no type mapping?");

            if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Binary))
            {
                PopulateBinaryParameter(parameter, type, DbType.Binary);
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Boolean))
            {
                parameter.DbType = DbType.Boolean;
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Byte))
            {
                parameter.DbType = DbType.Byte;
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.DateTime))
            {
                parameter.DbType = DbType.DateTime;
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Decimal))
            {
                PopulateDecimalParameter(parameter, type, DbType.Decimal);
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Double))
            {
                parameter.DbType = DbType.Double;
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Guid))
            {
                parameter.DbType = DbType.Guid;
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Single))
            {
                parameter.DbType = DbType.Single;
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Int16))
            {
                parameter.DbType = DbType.Int16;
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Int32))
            {
                parameter.DbType = DbType.Int32;
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Int64))
            {
                parameter.DbType = DbType.Int64;
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String))
            {
                PopulateStringParameter(parameter, type);
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Time))
            {
                throw ADP1.NotSupported(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, "Time"));
            }
            else if (TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.DateTimeOffset))
            {
                throw ADP1.NotSupported(EntityRes.GetString(EntityRes.ProviderDoesNotSupportType, "DateTimeOffset"));
            }
            else
            {
                ADP1.NotSupported(" UnKnown data type");
            }
        }

        private static void PopulateBinaryParameter(DbParameter parameter, TypeUsage type, DbType dbType)
        {
            // we do this only for Binary parameter. 
            // the function name is misleading.
            // 
            parameter.DbType = dbType;

            // For each facet, set the facet value only if we have it, note that it's possible to not have
            // it in the case the facet value is null
            SetParameterSize(parameter, type);
        }

        private static void PopulateDecimalParameter(DbParameter parameter, TypeUsage type, DbType dbType)
        {
            parameter.DbType = dbType;
            IDbDataParameter dataParameter = parameter;

            // For each facet, set the facet value only if we have it, note that it's possible to not have
            // it in the case the facet value is null
            byte precision;
            byte scale;
            if (TypeHelpers.TryGetPrecision(type, out precision))
            {
                dataParameter.Precision = precision;
            }

            if (TypeHelpers.TryGetScale(type, out scale))
            {
                dataParameter.Scale = scale;
            }
        }

        private static void PopulateStringParameter(DbParameter parameter, TypeUsage type)
        {
            // For each facet, set the facet value only if we have it, note that it's possible to not have
            // it in the case the facet value is null
            var unicode = true;
            var fixedLength = false;

            if (!TypeHelpers.TryGetIsFixedLength(type, out fixedLength))
            {
                // If we can't get the fixed length facet value, then default to fixed length = false
                fixedLength = false;
            }

            if (!TypeHelpers.TryGetIsUnicode(type, out unicode))
            {
                // If we can't get the unicode facet value, then default to unicode = true
                unicode = true;
            }

            if (fixedLength)
            {
                parameter.DbType = (unicode ? DbType.StringFixedLength : DbType.AnsiStringFixedLength);
            }
            else
            {
                parameter.DbType = (unicode ? DbType.String : DbType.AnsiString);
            }

            SetParameterSize(parameter, type);
        }

        private static void SetParameterSize(DbParameter parameter, TypeUsage type)
        {
            // only set the size if the parameter has a specific size value.
            Facet maxLengthFacet;
            if (type.Facets.TryGetValue(ProviderManifest.MaxLengthFacetName, true, out maxLengthFacet)
                && maxLengthFacet.Value != null)
            {
                // only set size if there is a specific size
                if (!Helper.IsUnboundedFacetValue(maxLengthFacet))
                {
                    parameter.Size = (int)maxLengthFacet.Value;
                }
            }
        }
    }
}
