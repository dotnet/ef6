// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration.Extensions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal static class EntitySetExtensions
    {
        public static string GetStoreModelBuilderMetadataProperty(this EntitySet entitySet, string name)
        {
            MetadataProperty metadataProperty;
            if (!entitySet.MetadataProperties.TryGetValue(
                SchemaManager.EntityStoreSchemaGeneratorNamespace + ":" + name,
                false,
                out metadataProperty))
            {
                return null;
            }

            return metadataProperty.Value as string;
        }
    }
}
