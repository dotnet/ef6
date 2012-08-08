// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    internal class PropertyEntryMetadataForMock : PropertyEntryMetadata
    {
        public PropertyEntryMetadataForMock()
            : base(typeof(object), typeof(object), "fake Name, mock Name property", true, true)
        {
        }
    }
}
