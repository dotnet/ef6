// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    internal class NavigationEntryMetadataForMock : NavigationEntryMetadata
    {
        public NavigationEntryMetadataForMock()
            : base(typeof(object), typeof(object), "fake Name, mock Name property", true)
        {
        }
    }
}
