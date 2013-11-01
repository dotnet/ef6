// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;

    internal static class EntitySetExtensions
    {
        public static object GetConfiguration(this EntitySet entitySet)
        {
            DebugCheck.NotNull(entitySet);

            return entitySet.Annotations.GetConfiguration();
        }

        public static void SetConfiguration(this EntitySet entitySet, object configuration)
        {
            DebugCheck.NotNull(entitySet);

            entitySet.GetMetadataProperties().SetConfiguration(configuration);
        }
    }
}
