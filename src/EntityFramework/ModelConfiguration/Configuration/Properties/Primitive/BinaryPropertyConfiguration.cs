// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Primitive
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Diagnostics.Contracts;

    internal class BinaryPropertyConfiguration : LengthPropertyConfiguration
    {
        public bool? IsRowVersion { get; set; }

        public BinaryPropertyConfiguration()
        {
        }

        private BinaryPropertyConfiguration(BinaryPropertyConfiguration source)
            : base(source)
        {
            Contract.Requires(source != null);

            IsRowVersion = source.IsRowVersion;
        }

        internal override PrimitivePropertyConfiguration Clone()
        {
            return new BinaryPropertyConfiguration(this);
        }

        internal override void Configure(EdmProperty property)
        {
            if (IsRowVersion != null)
            {
                ColumnType = ColumnType ?? "rowversion";
                ConcurrencyMode = ConcurrencyMode ?? EdmConcurrencyMode.Fixed;
                DatabaseGeneratedOption
                    = DatabaseGeneratedOption
                      ?? ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed;
                IsNullable = IsNullable ?? false;
                MaxLength = MaxLength ?? 8;
            }

            base.Configure(property);
        }

        internal override void Configure(
            IEnumerable<Tuple<DbEdmPropertyMapping, DbTableMetadata>> propertyMappings,
            DbProviderManifest providerManifest,
            bool allowOverride = false)
        {
            base.Configure(propertyMappings, providerManifest, allowOverride);

            propertyMappings
                .Each(
                    pm =>
                        {
                            if (IsRowVersion != null)
                            {
                                pm.Item1.Column.Facets.MaxLength = null;
                            }
                        });
        }

        internal override void CopyFrom(PrimitivePropertyConfiguration other)
        {
            base.CopyFrom(other);
            var strConfigRhs = other as BinaryPropertyConfiguration;
            if (strConfigRhs != null)
            {
                IsRowVersion = strConfigRhs.IsRowVersion;
            }
        }

        public override void FillFrom(PrimitivePropertyConfiguration other, bool inCSpace)
        {
            base.FillFrom(other, inCSpace);
            var strConfigRhs = other as BinaryPropertyConfiguration;
            if (strConfigRhs != null
                && IsRowVersion == null)
            {
                IsRowVersion = strConfigRhs.IsRowVersion;
            }
        }

        public override bool IsCompatible(PrimitivePropertyConfiguration other, bool InCSpace, out string errorMessage)
        {
            var binaryRhs = other as BinaryPropertyConfiguration;

            var baseIsCompatible = base.IsCompatible(other, InCSpace, out errorMessage);
            var isRowVersionIsCompatible = binaryRhs == null
                                           || IsCompatible(c => c.IsRowVersion, binaryRhs, ref errorMessage);

            return baseIsCompatible &&
                   isRowVersionIsCompatible;
        }
    }
}
