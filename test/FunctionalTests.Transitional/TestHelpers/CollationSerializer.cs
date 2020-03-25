// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    using System.Data.Entity.Infrastructure;

    public class CollationSerializer : IMetadataAnnotationSerializer
    {
        public string Serialize(string name, object value)
        {
            return ((CollationAttribute)value).CollationName;
        }

        public object Deserialize(string name, string value)
        {
            return new CollationAttribute(value);
        }
    }
}
