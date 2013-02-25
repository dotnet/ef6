// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Edm.Provider;

    /// <summary>
    ///     Provides singleton model TypeUsages for each DbType that can be expressed using a supported EDM type and appropriate facet values.
    ///     Used by EntityParameter.GetTypeUsage - if you add additional TypeUsage fields here, review the impact on that method.
    /// </summary>
    internal static class DbTypeMap
    {
        internal static readonly TypeUsage AnsiString = CreateType(
            PrimitiveTypeKind.String, new FacetValues
                                          {
                                              Unicode = false,
                                              FixedLength = false,
                                              MaxLength = (int?)null
                                          });

        internal static readonly TypeUsage AnsiStringFixedLength = CreateType(
            PrimitiveTypeKind.String, new FacetValues
                                          {
                                              Unicode = false,
                                              FixedLength = true,
                                              MaxLength = (int?)null
                                          });

        internal static readonly TypeUsage String = CreateType(
            PrimitiveTypeKind.String, new FacetValues
                                          {
                                              Unicode = true,
                                              FixedLength = false,
                                              MaxLength = (int?)null
                                          });

        internal static readonly TypeUsage StringFixedLength = CreateType(
            PrimitiveTypeKind.String, new FacetValues
                                          {
                                              Unicode = true,
                                              FixedLength = true,
                                              MaxLength = (int?)null
                                          });

        // XML parameters must not have a explicit size

        internal static readonly TypeUsage Xml = CreateType(
            PrimitiveTypeKind.String, new FacetValues
                                          {
                                              Unicode = true,
                                              FixedLength = false,
                                              MaxLength = (int?)null
                                          });

        internal static readonly TypeUsage Binary = CreateType(
            PrimitiveTypeKind.Binary, new FacetValues
                                          {
                                              MaxLength = (int?)null
                                          });

        internal static readonly TypeUsage Boolean = CreateType(PrimitiveTypeKind.Boolean);
        internal static readonly TypeUsage Byte = CreateType(PrimitiveTypeKind.Byte);
        internal static readonly TypeUsage DateTime = CreateType(PrimitiveTypeKind.DateTime);
        internal static readonly TypeUsage Date = CreateType(PrimitiveTypeKind.DateTime);

        internal static readonly TypeUsage DateTime2 = CreateType(
            PrimitiveTypeKind.DateTime, new FacetValues
                                            {
                                                Precision = (byte?)null
                                            });

        internal static readonly TypeUsage Time = CreateType(
            PrimitiveTypeKind.Time, new FacetValues
                                        {
                                            Precision = (byte?)null
                                        });

        internal static readonly TypeUsage DateTimeOffset = CreateType(
            PrimitiveTypeKind.DateTimeOffset, new FacetValues
                                                  {
                                                      Precision = (byte?)null
                                                  });

        // For decimal and money, in the case of precision == 0, we don't want any facets when picking the type so the
        // default type should be picked    
        internal static readonly TypeUsage Decimal = CreateType(
            PrimitiveTypeKind.Decimal, new FacetValues
                                           {
                                               Precision = (byte?)null,
                                               Scale = (byte?)null
                                           });

        // SQLBU 480928: Need to make currency a separate case once we enable money type
        internal static readonly TypeUsage Currency = CreateType(
            PrimitiveTypeKind.Decimal, new FacetValues
                                           {
                                               Precision = (byte?)null,
                                               Scale = (byte?)null
                                           });

        internal static readonly TypeUsage Double = CreateType(PrimitiveTypeKind.Double);
        internal static readonly TypeUsage Guid = CreateType(PrimitiveTypeKind.Guid);
        internal static readonly TypeUsage Int16 = CreateType(PrimitiveTypeKind.Int16);
        internal static readonly TypeUsage Int32 = CreateType(PrimitiveTypeKind.Int32);
        internal static readonly TypeUsage Int64 = CreateType(PrimitiveTypeKind.Int64);
        internal static readonly TypeUsage Single = CreateType(PrimitiveTypeKind.Single);
        internal static readonly TypeUsage SByte = CreateType(PrimitiveTypeKind.SByte);

        internal static bool TryGetModelTypeUsage(DbType dbType, out TypeUsage modelType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                    modelType = AnsiString;
                    break;

                case DbType.AnsiStringFixedLength:
                    modelType = AnsiStringFixedLength;
                    break;

                case DbType.String:
                    modelType = String;
                    break;

                case DbType.StringFixedLength:
                    modelType = StringFixedLength;
                    break;

                case DbType.Xml:
                    modelType = Xml;
                    break;

                case DbType.Binary:
                    modelType = Binary;
                    break;

                case DbType.Boolean:
                    modelType = Boolean;
                    break;

                case DbType.Byte:
                    modelType = Byte;
                    break;

                case DbType.DateTime:
                    modelType = DateTime;
                    break;

                case DbType.Date:
                    modelType = Date;
                    break;

                case DbType.DateTime2:
                    modelType = DateTime2;
                    break;

                case DbType.Time:
                    modelType = Time;
                    break;

                case DbType.DateTimeOffset:
                    modelType = DateTimeOffset;
                    break;

                case DbType.Decimal:
                    modelType = Decimal;
                    break;

                case DbType.Currency:
                    modelType = Currency;
                    break;

                case DbType.Double:
                    modelType = Double;
                    break;

                case DbType.Guid:
                    modelType = Guid;
                    break;

                case DbType.Int16:
                    modelType = Int16;
                    break;

                case DbType.Int32:
                    modelType = Int32;
                    break;

                case DbType.Int64:
                    modelType = Int64;
                    break;

                case DbType.Single:
                    modelType = Single;
                    break;

                case DbType.SByte:
                    modelType = SByte;
                    break;

                case DbType.VarNumeric:
                    modelType = null;
                    break;

                default:
                    modelType = null;
                    break;
            }

            return (modelType != null);
        }

        private static TypeUsage CreateType(PrimitiveTypeKind type)
        {
            return CreateType(type, new FacetValues());
        }

        private static TypeUsage CreateType(PrimitiveTypeKind type, FacetValues facets)
        {
            var primitiveType = EdmProviderManifest.Instance.GetPrimitiveType(type);
            var typeUsage = TypeUsage.Create(primitiveType, facets);
            return typeUsage;
        }
    }
}
