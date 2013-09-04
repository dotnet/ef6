// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;

    /// <summary>
    /// Base class for items in the mapping space (DataSpace.CSSpace)
    /// </summary>
    public abstract class MappingItem
    {
        private bool _readOnly;
        private readonly List<MetadataProperty> _annotations = new List<MetadataProperty>();

        internal bool IsReadOnly
        {
            get { return _readOnly; }
        }

        internal IList<MetadataProperty> Annotations
        {
            get { return _annotations; }
        }

        internal void SetReadOnly()
        {
            _readOnly = true;
        }

        internal void ThrowIfReadOnly()
        {
            if (IsReadOnly)
            {
                throw new InvalidOperationException(Strings.OperationOnReadOnlyItem);
            }
        }
    }
}
