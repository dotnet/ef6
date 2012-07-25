// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Edm.Db.Mapping
{
    using System.Data.Entity.Edm.Db.Mapping;
    using System.Data.Entity.ModelConfiguration.Edm.Common;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;

    internal static class DbAssociationSetMappingExtensions
    {
        public static DbAssociationSetMapping Initialize(this DbAssociationSetMapping associationSetMapping)
        {
            Contract.Requires(associationSetMapping != null);

            associationSetMapping.SourceEndMapping = new DbAssociationEndMapping();
            associationSetMapping.TargetEndMapping = new DbAssociationEndMapping();

            return associationSetMapping;
        }

        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        public static object GetConfiguration(this DbAssociationSetMapping associationSetMapping)
        {
            Contract.Requires(associationSetMapping != null);

            return associationSetMapping.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this DbAssociationSetMapping associationSetMapping, object configuration)
        {
            Contract.Requires(associationSetMapping != null);

            associationSetMapping.Annotations.SetConfiguration(configuration);
        }
    }
}
