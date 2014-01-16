// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Represents the base item class for all the mapping metadata
    /// </summary>
    public abstract class MappingBase : GlobalItem
    {
        internal MappingBase()
            : base(MetadataFlags.Readonly)
        {
        }

        internal MappingBase(MetadataFlags flags)
            : base(flags)
        {
        }

        // <summary>
        // Returns the Item that is being mapped either for ES or OE spaces.
        // The EDM type will be an EntityContainer type in ES mapping case.
        // In the OE mapping case it could be any type.
        // </summary>
        internal abstract MetadataItem EdmItem { get; }
    }
}
