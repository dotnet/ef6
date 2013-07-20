// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal static class MetadataItemHelper
    {
        internal const string SchemaErrorsMetadataPropertyName = "EdmSchemaErrors";
        internal const string SchemaInvalidMetadataPropertyName = "EdmSchemaInvalid";

        public static bool IsInvalid(MetadataItem instance)
        {
            Debug.Assert(instance != null, "instance != null");

            MetadataProperty property;
            if (!instance.MetadataProperties.TryGetValue(SchemaInvalidMetadataPropertyName, false, out property)
                || property == null)
            {
                return false;
            }

            return (bool)property.Value;
        }

        public static bool HasSchemaErrors(MetadataItem instance)
        {
            Debug.Assert(instance != null, "instance != null");

            return instance.MetadataProperties.Contains(SchemaErrorsMetadataPropertyName);
        }

        public static IEnumerable<EdmSchemaError> GetSchemaErrors(MetadataItem instance)
        {
            Debug.Assert(instance != null, "instance != null");

            MetadataProperty property;
            if (!instance.MetadataProperties.TryGetValue(SchemaErrorsMetadataPropertyName, false, out property)
                || property == null)
            {
                return Enumerable.Empty<EdmSchemaError>();
            }

            return (IEnumerable<EdmSchemaError>)property.Value;
        }
    }
}
